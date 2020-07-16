#if NAMED_PIPE_AVAILABLE
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace StatsdClient
{
    internal class NamedPipeTransport : ITransport
    {
        private readonly NamedPipeClientStream _namedPipe;
        private readonly TimeSpan _timeout;
        private byte[] _internalbuffer = new byte[0];

        // As SpinLock is a struct, if it is marked as `readonly`, each time it is used
        // a new copy is created which leads to the error:
        // System.Threading.SynchronizationLockException : The calling thread does not hold the lock.
        private SpinLock _lock = new SpinLock(enableThreadOwnerTracking: true);

        public NamedPipeTransport(string pipeName, TimeSpan? timeout = null)
        {
            _namedPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
            _timeout = timeout ?? TimeSpan.FromSeconds(2);
        }

        public TransportType TransportType => TransportType.NamedPipe;

        public string TelemetryClientTransport => "named_pipe";

        public bool Send(byte[] buffer, int length)
        {
            var gotLock = false;
            try
            {
                _lock.Enter(ref gotLock);
                if (_internalbuffer.Length < length + 1)
                {
                    _internalbuffer = new byte[length + 1];
                }

                // Server expects messages to end with '\n'
                Array.Copy(buffer, 0, _internalbuffer, 0, length);
                _internalbuffer[length] = (byte)'\n';

                return SendBuffer(_internalbuffer, length + 1, allowRetry: true);
            }
            finally
            {
                if (gotLock)
                {
                    _lock.Exit();
                }
            }
        }

        public void Dispose()
        {
            _namedPipe.Dispose();
        }

        private bool SendBuffer(byte[] buffer, int length, bool allowRetry)
        {
            try
            {
                if (!_namedPipe.IsConnected)
                {
                    _namedPipe.Connect((int)_timeout.TotalMilliseconds);
                }
            }
            catch (TimeoutException)
            {
                return false;
            }

            var cts = new CancellationTokenSource(_timeout);

            try
            {
                // WriteAsync overload with a CancellationToken instance seems to not work.
                _namedPipe.WriteAsync(buffer, 0, length).Wait(cts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (IOException)
            {
                // When the server disconnects, IOException is raised with the message "Pipe is broken".
                // In this case, we try to reconnect once.
                if (allowRetry)
                {
                    return SendBuffer(buffer, length, allowRetry: false);
                }

                return false;
            }
        }
    }
}
#endif
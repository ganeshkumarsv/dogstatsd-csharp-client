using System;
using StatsdClient.Statistic;
using StatsdClient.Worker;

namespace StatsdClient.Bufferize
{
    /// <summary>
    /// StatsBufferize bufferizes metrics before sending them.
    /// </summary>
    internal class StatsBufferize : IDisposable
    {
        private readonly AsynchronousWorker<Stats> _worker;

        public StatsBufferize(
            StatsRouter statsRouter,
            int workerMaxItemCount,
            TimeSpan? blockingQueueTimeout,
            TimeSpan maxIdleWaitBeforeSending)
        {
            var handler = new WorkerHandler(statsRouter, maxIdleWaitBeforeSending);

            // `handler` (and also `statsRouter`) do not need to be thread safe as long as `workerThreadCount` is 1.
            this._worker = new AsynchronousWorker<Stats>(
                () => new Stats(),
                handler,
                new Waiter(),
                workerThreadCount: 1,
                workerMaxItemCount,
                blockingQueueTimeout);
        }

        public void Send(Stats serializedMetric) => this._worker.Enqueue(serializedMetric);

        public bool TryDequeueFromPool(out Stats value) => _worker.TryDequeueFromPool(out value);

        public void Flush()
        {
            this._worker.Flush();
        }

        public void Dispose()
        {
            this._worker.Dispose();
        }

        private class WorkerHandler : IAsynchronousWorkerHandler<Stats>
        {
            private readonly StatsRouter _statsRouter;
            private readonly TimeSpan _maxIdleWaitBeforeSending;
            private readonly System.Diagnostics.Stopwatch _stopwatch;
            private bool _resetTimer;

            public WorkerHandler(StatsRouter statsRouter, TimeSpan maxIdleWaitBeforeSending)
            {
                _stopwatch = new System.Diagnostics.Stopwatch();
                _statsRouter = statsRouter;
                _maxIdleWaitBeforeSending = maxIdleWaitBeforeSending;
            }

            public void OnNewValue(Stats stats)
            {
                _statsRouter.Route(stats);
                _resetTimer = true;
            }

            public bool OnIdle()
            {
                if (_resetTimer)
                {
                    _stopwatch.Restart();
                    _resetTimer = false;
                }

                if (_stopwatch.ElapsedMilliseconds > _maxIdleWaitBeforeSending.TotalMilliseconds)
                {
                    this._statsRouter.OnIdle();

                    return true;
                }

                return true;
            }

            public void Flush()
            {
                this._statsRouter.Flush();
            }
        }
    }
}

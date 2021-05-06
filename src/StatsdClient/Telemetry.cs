using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using StatsdClient.Bufferize;
using StatsdClient.Statistic;
using StatsdClient.Transport;

namespace StatsdClient
{
    /// <summary>
    /// Telemetry sends telemetry metrics.
    /// </summary>
    internal class Telemetry : IDisposable, ITelemetryCounters
    {
        private static string _telemetryPrefix = "datadog.dogstatsd.client.";
        private readonly Timer _optionalTimer;
        private readonly string[] _optionalTags;
        private readonly MetricSerializer _optionalMetricSerializer;
        private readonly ITransport _optionalTransport;
        private readonly Dictionary<MetricType, ValueWithTags> _aggregatedContexts = new Dictionary<MetricType, ValueWithTags>();

        private int _metricsSent;
        private int _eventsSent;
        private int _serviceChecksSent;
        private int _bytesSent;
        private int _bytesDropped;
        private int _packetsSent;
        private int _packetsDropped;
        private int _packetsDroppedQueue;

        // This constructor does not send telemetry.
        public Telemetry()
        {
        }

        public Telemetry(
            MetricSerializer metricSerializer,
            string assemblyVersion,
            TimeSpan flushInterval,
            ITransport transport,
            string[] globalTags)
        {
            _optionalMetricSerializer = metricSerializer;
            _optionalTransport = transport;

            var transportStr = transport.TelemetryClientTransport;
            var optionalTags = new List<string> { "client:csharp", $"client_version:{assemblyVersion}", $"client_transport:{transportStr}" };
            optionalTags.AddRange(globalTags);
            _optionalTags = optionalTags.ToArray();
            _aggregatedContexts.Add(MetricType.Gauge, new ValueWithTags(_optionalTags, "metrics_type:gauge"));
            _aggregatedContexts.Add(MetricType.Count, new ValueWithTags(_optionalTags, "metrics_type:count"));
            _aggregatedContexts.Add(MetricType.Set, new ValueWithTags(_optionalTags, "metrics_type:set"));

            _optionalTimer = new Timer(
                _ => Flush(),
                null,
                flushInterval,
                flushInterval);
        }

        public static string MetricsMetricName => _telemetryPrefix + "metrics";

        public static string EventsMetricName => _telemetryPrefix + "events";

        public static string ServiceCheckMetricName => _telemetryPrefix + "service_checks";

        public static string BytesSentMetricName => _telemetryPrefix + "bytes_sent";

        public static string BytesDroppedMetricName => _telemetryPrefix + "bytes_dropped";

        public static string PacketsSentMetricName => _telemetryPrefix + "packets_sent";

        public static string PacketsDroppedMetricName => _telemetryPrefix + "packets_dropped";

        public static string PacketsDroppedQueueMetricName => _telemetryPrefix + "packets_dropped_queue";

        public static string AggregatedContextByTypeName => _telemetryPrefix + "aggregated_context_by_type";

        public int MetricsSent => _metricsSent;

        public int EventsSent => _eventsSent;

        public int ServiceChecksSent => _serviceChecksSent;

        public int BytesSent => _bytesSent;

        public int BytesDropped => _bytesDropped;

        public int PacketsSent => _packetsSent;

        public int PacketsDropped => _packetsDropped;

        public int PacketsDroppedQueue => _packetsDroppedQueue;

        public void Flush()
        {
            try
            {
                SendMetric(MetricsMetricName, Interlocked.Exchange(ref _metricsSent, 0));
                SendMetric(EventsMetricName, Interlocked.Exchange(ref _eventsSent, 0));
                SendMetric(ServiceCheckMetricName, Interlocked.Exchange(ref _serviceChecksSent, 0));
                SendMetric(BytesSentMetricName, Interlocked.Exchange(ref _bytesSent, 0));
                SendMetric(BytesDroppedMetricName, Interlocked.Exchange(ref _bytesDropped, 0));
                SendMetric(PacketsSentMetricName, Interlocked.Exchange(ref _packetsSent, 0));
                SendMetric(PacketsDroppedMetricName, Interlocked.Exchange(ref _packetsDropped, 0));
                SendMetric(PacketsDroppedQueueMetricName, Interlocked.Exchange(ref _packetsDroppedQueue, 0));

                foreach (var kvp in _aggregatedContexts)
                {
                    var metricType = kvp.Key;
                    var metricWithTags = kvp.Value;

                    SendMetricWithTags(
                        AggregatedContextByTypeName,
                        metricWithTags.Tags,
                        _aggregatedContexts[metricType].InterlockedExchange(0));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void OnMetricSent()
        {
            Interlocked.Increment(ref _metricsSent);
        }

        public void OnEventSent()
        {
            Interlocked.Increment(ref _eventsSent);
        }

        public void OnServiceCheckSent()
        {
            Interlocked.Increment(ref _serviceChecksSent);
        }

        public void OnPacketSent(int packetSize)
        {
            Interlocked.Increment(ref _packetsSent);
            Interlocked.Add(ref _bytesSent, packetSize);
        }

        public void OnPacketDropped(int packetSize)
        {
            Interlocked.Increment(ref _packetsDropped);
            Interlocked.Add(ref _bytesDropped, packetSize);
        }

        public void OnPacketsDroppedQueue()
        {
            Interlocked.Increment(ref _packetsDroppedQueue);
        }

        public void OnAggregatedContextFlush(MetricType metricType, int count)
        {
            if (_aggregatedContexts.TryGetValue(metricType, out var aggregatedContext))
            {
                aggregatedContext.InterlockedAdd(count);
            }
        }

        public void Dispose()
        {
            _optionalTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _optionalTimer?.Dispose();
        }

        private void SendMetricWithTags(string metricName, string[] tags, int value)
        {
            if (_optionalTransport != null && _optionalMetricSerializer != null)
            {
                var serializedMetric = new SerializedMetric();
                var metricStats = new StatsMetric
                {
                    MetricType = MetricType.Count,
                    StatName = metricName,
                    NumericValue = value,
                    SampleRate = 1.0,
                    Tags = tags,
                };
                _optionalMetricSerializer.SerializeTo(ref metricStats, serializedMetric);
                var bytes = BufferBuilder.GetBytes(serializedMetric.ToString());
                _optionalTransport.Send(bytes, bytes.Length);
            }
        }

        private void SendMetric(string metricName, int value)
        {
            SendMetricWithTags(metricName, _optionalTags, value);
        }

        private class ValueWithTags
        {
            private int value;

            public ValueWithTags(string[] tags, string metricTypeTag)
            {
                this.Tags = new string[tags.Length + 1];
                Array.Copy(tags, this.Tags, tags.Length);
                this.Tags[this.Tags.Length - 1] = metricTypeTag;
            }

            public string[] Tags { get; }

            public void InterlockedAdd(int count)
            {
                Interlocked.Add(ref value, count);
            }

            public int InterlockedExchange(int newValue)
            {
                return Interlocked.Exchange(ref value, newValue);
            }
        }
    }
}
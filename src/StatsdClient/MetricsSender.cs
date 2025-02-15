﻿using System;
using StatsdClient.Bufferize;
using StatsdClient.Statistic;

namespace StatsdClient
{
    internal class MetricsSender
    {
        private readonly Telemetry _optionalTelemetry;
        private readonly StatsBufferize _statsBufferize;
        private readonly bool _truncateIfTooLong;
        private readonly IStopWatchFactory _stopwatchFactory;
        private readonly IRandomGenerator _randomGenerator;

        internal MetricsSender(
                    StatsBufferize statsBufferize,
                    IRandomGenerator randomGenerator,
                    IStopWatchFactory stopwatchFactory,
                    Telemetry optionalTelemetry,
                    bool truncateIfTooLong)
        {
            _stopwatchFactory = stopwatchFactory;
            _statsBufferize = statsBufferize;
            _randomGenerator = randomGenerator;
            _optionalTelemetry = optionalTelemetry;
            _truncateIfTooLong = truncateIfTooLong;
        }

        public void SendEvent(string title, string text, string alertType = null, string aggregationKey = null, string sourceType = null, int? dateHappened = null, string priority = null, string hostname = null, string[] tags = null, bool truncateIfTooLong = false)
        {
            if (TryDequeueStats(out var stats))
            {
                stats.Kind = StatsKind.Event;
                stats.Event.Tags = tags;
                stats.Event.Title = title;
                stats.Event.Text = text;
                stats.Event.AlertType = alertType;
                stats.Event.AggregationKey = aggregationKey;
                stats.Event.SourceType = sourceType;
                stats.Event.DateHappened = dateHappened;
                stats.Event.Priority = priority;
                stats.Event.Hostname = hostname;
                stats.Event.TruncateIfTooLong = truncateIfTooLong || _truncateIfTooLong;

                Send(stats);
                _optionalTelemetry?.OnEventSent();
            }
        }

        public void SendServiceCheck(string name, int status, int? timestamp = null, string hostname = null, string[] tags = null, string serviceCheckMessage = null, bool truncateIfTooLong = false)
        {
            if (TryDequeueStats(out var stats))
            {
                stats.Kind = StatsKind.ServiceCheck;
                stats.ServiceCheck.Tags = tags;
                stats.ServiceCheck.Name = name;
                stats.ServiceCheck.Status = status;
                stats.ServiceCheck.Timestamp = timestamp;
                stats.ServiceCheck.Hostname = hostname;
                stats.ServiceCheck.ServiceCheckMessage = serviceCheckMessage;
                stats.ServiceCheck.TruncateIfTooLong = truncateIfTooLong || _truncateIfTooLong;

                Send(stats);
                _optionalTelemetry?.OnServiceCheckSent();
            }
        }

        public void SendMetric(MetricType metricType, string name, double value, double sampleRate = 1.0, string[] tags = null)
        {
            if (metricType == MetricType.Set)
            {
                throw new ArgumentException($"{nameof(SendMetric)} does not support `MetricType.Set`.");
            }

            if (_randomGenerator.ShouldSend(sampleRate))
            {
                if (TryDequeueStats(out var stats))
                {
                    stats.Kind = StatsKind.Metric;
                    stats.Metric.Tags = tags;
                    stats.Metric.MetricType = metricType;
                    stats.Metric.StatName = name;
                    stats.Metric.SampleRate = sampleRate;
                    stats.Metric.NumericValue = value;

                    Send(stats);
                    _optionalTelemetry?.OnMetricSent();
                }
            }
        }

        public void SendSetMetric(string name, string value, double sampleRate = 1.0, string[] tags = null)
        {
            if (_randomGenerator.ShouldSend(sampleRate))
            {
                if (TryDequeueStats(out var stats))
                {
                    stats.Kind = StatsKind.Metric;
                    stats.Metric.Tags = tags;
                    stats.Metric.MetricType = MetricType.Set;
                    stats.Metric.StatName = name;
                    stats.Metric.SampleRate = sampleRate;
                    stats.Metric.StringValue = value;

                    Send(stats);
                    _optionalTelemetry?.OnMetricSent();
                }
            }
        }

        public void Send(Action actionToTime, string statName, double sampleRate = 1.0, string[] tags = null)
        {
            var stopwatch = _stopwatchFactory.Get();

            try
            {
                stopwatch.Start();
                actionToTime();
            }
            finally
            {
                stopwatch.Stop();
                SendMetric(MetricType.Timing, statName, stopwatch.ElapsedMilliseconds(), sampleRate, tags);
            }
        }

        private bool TryDequeueStats(out Stats stats)
        {
            if (_statsBufferize.TryDequeueFromPool(out stats))
            {
                return true;
            }

            _optionalTelemetry?.OnPacketsDroppedQueue();
            return false;
        }

        private void Send(Stats metricFields) => _statsBufferize.Send(metricFields);
    }
}
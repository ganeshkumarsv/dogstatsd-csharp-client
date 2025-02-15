﻿using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StatsdClient.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009558fd81ea0e330858198ae6c860c0c9fd2d9df3e5f2069434649e4ec61c9ceb9744d2a3fd82518d90abb5cbcefb6292e9d227d5854bd07dbd8884d129350c95c7742d499dfc4961223b35326e203c5924e413a2385a7aa7c704432e9101bb946da201977df2b992c25d0fb77645c1ac5bc29cde7bc8e5d054b78bd9c6727497")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2,PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
[assembly: InternalsVisibleTo("StatsdClient.Benchmarks,PublicKey=00240000048000009400000006020000002400005253413100040000010001009558fd81ea0e330858198ae6c860c0c9fd2d9df3e5f2069434649e4ec61c9ceb9744d2a3fd82518d90abb5cbcefb6292e9d227d5854bd07dbd8884d129350c95c7742d499dfc4961223b35326e203c5924e413a2385a7aa7c704432e9101bb946da201977df2b992c25d0fb77645c1ac5bc29cde7bc8e5d054b78bd9c6727497")]

namespace StatsdClient
{
    /// <summary>
    /// The status of the service check.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Status OK
        /// </summary>
        OK = 0,

        /// <summary>
        /// Status Warning
        /// </summary>
        WARNING = 1,

        /// <summary>
        /// Status Critical
        /// </summary>
        CRITICAL = 2,

        /// <summary>
        /// Status Unknown
        /// </summary>
        UNKNOWN = 3,
    }

    /// <summary>
    /// DogStatsd is a collection of static methods that provide the same feature as DogStatsdService.
    /// </summary>
    public static class DogStatsd
    {
        private static readonly DogStatsdService _dogStatsdService = new DogStatsdService();

        /// <summary>
        /// Gets the telemetry counters
        /// </summary>
        /// <value>The telemetry counters.</value>
        public static ITelemetryCounters TelemetryCounters => _dogStatsdService.TelemetryCounters;

        /// <summary>
        /// Configures the instance.
        /// Must be called before any other methods.
        /// </summary>
        /// <param name="config">The value of the config.</param>
        public static void Configure(StatsdConfig config) => _dogStatsdService.Configure(config);

        /// <summary>
        /// Records an event.
        /// </summary>
        /// <param name="title">The title of the event.</param>
        /// <param name="text">The text body of the event.</param>
        /// <param name="alertType">error, warning, success, or info (defaults to info).</param>
        /// <param name="aggregationKey">A key to use for aggregating events.</param>
        /// <param name="sourceType">The source type name.</param>
        /// <param name="dateHappened">The epoch timestamp for the event (defaults to the current time from the DogStatsD server).</param>
        /// <param name="priority">Specifies the priority of the event (normal or low).</param>
        /// <param name="hostname">The name of the host.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Event(
            string title,
            string text,
            string alertType = null,
            string aggregationKey = null,
            string sourceType = null,
            int? dateHappened = null,
            string priority = null,
            string hostname = null,
            string[] tags = null)
            =>
                _dogStatsdService.Event(
                    title: title,
                    text: text,
                    alertType: alertType,
                    aggregationKey: aggregationKey,
                    sourceType: sourceType,
                    dateHappened: dateHappened,
                    priority: priority,
                    hostname: hostname,
                    tags: tags);

        /// <summary>
        /// Adjusts the specified counter by a given delta.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">A given delta.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Counter(string statName, double value, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Counter(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Increments the specified counter.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The amount of increment.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Increment(string statName, int value = 1, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Increment(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Decrements the specified counter.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The amount of decrement.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Decrement(string statName, int value = 1, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Decrement(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records the latest fixed value for the specified named gauge.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value of the gauge.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Gauge(string statName, double value, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Gauge(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records a value for the specified named histogram.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value of the histogram.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Histogram(string statName, double value, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Histogram(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records a value for the specified named distribution.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value of the distribution.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Distribution(string statName, double value, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Distribution(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records a value for the specified set.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public static void Set<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Set<T>(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records a value for the specified set.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Set(string statName, string value, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Set(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records an execution time in milliseconds.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The time in millisecond.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Timer(string statName, double value, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Timer(statName: statName, value: value, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Creates a timer that records the execution time until Dispose is called on the returned value.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <returns>A disposable object that records the execution time until Dispose is called.</returns>
        public static IDisposable StartTimer(string name, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.StartTimer(name: name, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records an execution time for the given action.
        /// </summary>
        /// <param name="action">The given action.</param>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public static void Time(Action action, string statName, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Time(action: action, statName: statName, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records an execution time for the given function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of the returned value of <paramref name="func"/>.</typeparam>
        /// <returns>The returned value of <paramref name="func"/>.</returns>
        public static T Time<T>(Func<T> func, string statName, double sampleRate = 1.0, string[] tags = null) =>
            _dogStatsdService.Time<T>(func: func, statName: statName, sampleRate: sampleRate, tags: tags);

        /// <summary>
        /// Records a run status for the specified named service check.
        /// </summary>
        /// <param name="name">The name of the service check.</param>
        /// <param name="status">A constant describing the service status.</param>
        /// <param name="timestamp">The epoch timestamp for the service check (defaults to the current time from the DogStatsD server).</param>
        /// <param name="hostname">The hostname to associate with the service check.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <param name="message">Additional information or a description of why the status occurred.</param>
        public static void ServiceCheck(
            string name,
            Status status,
            int? timestamp = null,
            string hostname = null,
            string[] tags = null,
            string message = null) =>
                _dogStatsdService.ServiceCheck(name, status, timestamp, hostname, tags, message);

        /// <summary>
        /// Flushes all metrics.
        /// </summary>
        /// <param name="flushTelemetry">The value indicating whether the telemetry must be flushed.</param>
        public static void Flush(bool flushTelemetry = true)
        {
            _dogStatsdService.Flush(flushTelemetry);
        }

        /// <summary>
        /// Disposes the instance of DogStatsdService.
        /// Flushes all metrics.
        /// </summary>
        public static void Dispose()
        {
            _dogStatsdService.Dispose();
        }
    }
}

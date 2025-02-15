CHANGELOG
=========

# 7.0.0 / 10-13-2021
## Breaking changes
### Enable client-side aggregation by default for simple metric types. See [#172][].
By default, metrics are aggregated before they are sent. For example, instead of sending 3 times `my_metric:10|c|#tag1:value`, DogStatsD client sends `my_metric:30|c|#tag1:value` once. You can disable client-side aggregation by setting `ClientSideAggregation` property to `null`.
For more details about how client-side aggregation works see [#134][].

Enabling client-side aggregation has the benefit of reducing the network usage and also reducing the load for DogStatsD server (Core Agent).

When an application sends a lot of different contexts but each context appear with a very low frequency, then enabling client-side aggregation may take more memory and more CPU. A context identifies a metric name, a tag sets and a metric type. The metric `datadog.dogstatsd.client.aggregated_context` reported by DogStatsD C# client counts the number of contexts in memory used for client-side aggregation. There is also the metric `datadog.dogstatsd.client.metrics_by_type` that represents the number of metrics submitted by the client before aggregation. 

### Set good default values for UDS and UDP buffer sizes. See [#170][].
This PR changes the default values for unix domain socket and UDP buffer sizes.
In most cases, this change should work out of the box. Unlike Windows and Linux, on some operating systems like MacOS, the maximum unix domain socket buffer size is lower than `8192`. For these systems you have to set `StatsdMaxUnixDomainSocketPacketSize` to the maximum supported value.

## Changes
* [BUGFIX] Update links to https://docs.datadoghq.com. See [#171][].
* [IMPROVEMENT] Add end of line separator after each message. See [#169][].
* [IMPROVEMENT] Add benchmark for client side aggregation. See [#168][].
* [BUGFIX] Fix the implementation of MetricStatsKey.GetHashCode(). See [#167][].
* [IMPROVEMENT] Update client side flush interval from 3s to 2s. See [#166][].
* [BUGFIX] For event, use the size of the title and the text in UTF8. See [#165][].
* [IMPROVEMENT] Performance improvements. See [#164][].
* [FEATURE] Add benchmarks. See [#163][].
* [IMPROVEMENT] Improve performance. See [#162][].
* [FEATURE] Add the telemetry metric aggregated_context_by_type. See [#161][].
* [IMPROVEMENT] Minor grammar update in comments. See [#160][] (Thanks [@shreyamsh][]).
* [IMPROVEMENT] Use Thread.Sleep instead of Task.Delay when possible. See [#159][] (Thanks [@kevingosse][]).
* [IMPROVEMENT] Remove System.Net.NameResolution for netstandard2.0. See [#155][] (Thanks [@fjmorel ][]).
* [IMPROVEMENT] Use dedicated threads for background workers. See [#151][] (Thanks [@kevingosse][]).

# 6.0.0 / 11-23-2020
## Breaking changes.
* Methods `Counter`, `Gauge`, `Histogram`, `Distribution` and `Timer` from `DogStatsdService` and `DogStatsd` are not generic methods anymore. (See https://github.com/DataDog/dogstatsd-csharp-client/pull/133/commits/ab18f9572de3bfe76fb95b5fce14d6ee965b62d4)
* The following obsolete code is removed:
  * Remove variables `StatsdConfig.DD_ENTITY_ID_ENV_VAR`, `StatsdConfig.DD_DOGSTATSD_PORT_ENV_VAR` and `StatsdConfig.DD_AGENT_HOST_ENV_VAR`
  * Visibility change from `public` to `internal` for: `ICommandType`, `IRandomGenerator`, `IStopWatchFactory`, `IStopwatch`, `MetricsTimer`, `RandomGenerator`, `StopWatchFactory`, `Stopwatch`, `ThreadSafeRandom` and `StatsdUDP`.
  * Remove `Statsd`, `IStatsd` and `IStatsdUDP`.
* Rename environment variable `DD_AGENT_PIPE_NAME` to `DD_DOGSTATSD_PIPE_NAME`.

## Changes 
* [IMPROVEMENT] Add `Flush` method. See [#144][].
* [FEATURE] Add client side aggregation for Count, Gauge and Set metrics. See [#133][], [#134][] and [#143][].
* [FEATURE] Add support for universal service tagging. See [#139][] (Thanks [@kevingosse][])
* [BUGFIX] Work around 64 bit RyuJIT ThreadAbortException bug on the .NET Framework. See [#137][] (Thanks [@jdasilva-olo][]).


# 5.1.0 / 09-07-2020
* [IMPROVEMENT] Reduce the memory allocations. See [#123][], [#124][], [#127][] and [#132][].
* [IMPROVEMENT] Add a telemetry end point. See [#130][].
* [IMPROVEMENT] Add the support for .NET framework 4.5.0. See [#128][].

# 5.0.2 / 05-29-2020
* [BUGFIX] Fix an issue where the client cannot send metrics after the DogStatsd server restart. See [#125][].

# 5.0.1 / 05-19-2020
* [BUGFIX] Fix System.Net.Sockets.SocketException when telemetry is enabled and Statsd server is not up. See [#120][].
* [BUGFIX] Fix high CPU usage. See [#121][].

# 5.0.0 / 05-13-2020
Significant improvements of `DogStatsdService` and `DogStatsd` performance.

## Breaking changes
**You must call `DogStatsdService.Dispose()` or `DogStatsd.Dispose()` before your program termination in order to flush metrics not yet sent.** 
`Statsd` is marked as obsolete.

## Changes 
* [IMPROVEMENT] Both `DogStatsdService` and `DogStatsd` methods do not block anymore and batch several metrics automatically in one UDP or UDS message. See [#108][] and [#109][].
* [IMPROVEMENT] Send telemetry metrics. See [#110][] and [#114][].
* [IMPROVEMENT] Enable StyleCop. See [#111][], [#112][] and [#113][].

# 4.0.1 / 02-11-2020
* [BUGFIX] Fix `System.ArgumentException: unixSocket must start with unix://` when using the `DD_AGENT_HOST` environment variable with UDS support. See [this comment](https://github.com/DataDog/dogstatsd-csharp-client/issues/85#issuecomment-581371860) (Thanks [@danopia][])

# 4.0.0 / 01-03-2020
## Breaking changes
Version `3.4.0` uses a strong-named assembly that may introduce a [breaking change](https://github.com/DataDog/dogstatsd-csharp-client/pull/96#issuecomment-561379859).
This major version change makes this breaking change explicit. No other breaking changes are expected.

## Changes 
* [IMPROVEMENT] Add Async methods to Statsd. See [#59][] (Thanks [@alistair][])
* [IMPROVEMENT] Add Unix domain socket support. See [#92][]

# 3.4.0 / 11-15-2019

* [IMPROVEMENT] Use a strong-named assembly. See [#96][] (Thanks [@carlreid][])


# 3.3.0 / 04-05-2019

* [FEATURE] Option to set global tags that are added to every statsd call. See [#3][], [#78][] (Thanks [@chriskinsman][])
* [IMPROVEMENT] Configure the client with environment variables. See [#78][]


# 3.2.0 / 10-18-2018

* [BUGFIX] Fix an issue causing the `StartTimer` method to ignore non static `DogStatsdService` instance configurations. See [#62][], [#63][] (Thanks [@jpasichnyk][])
* [BUGFIX] Prevent the static API from being configured more than once to avoid race conditions. See [#66][] (Thanks [@nrjohnstone][])
* [BUGFIX] Set a default value for `tags` in the `Decrement` method similar to `Increment`. See [#60][], [#61][] (Thanks [@sqdk][])
* [FEATURE] Add support for DogStatsD distribution. See [#65][]

# 3.1.0 / 11-16-2017

## Supported target framework versions

DogStatsD-CSharp-Client `3.1.0` supports the following platforms:
* .NET Standard 1.3
* .NET Standard 1.6
* .NET Core Application 1.1
* .NET Core Application 2.0
* .NET Framework 4.5.1
* .NET Framework 4.6.1

## Changes

* [BUGFIX] `DogStatsdService` implements `IDogStatsd`. See [#43][], [#54][]
* [BUGFIX] Fix IP host name resolution when IPv6 addresses are available. See [#50][] (Thanks [@DanielVukelich][])
* [IMPROVEMENT] Add `IDisposable` interface to `DogStatsdService` to manage the release of resources. See [#44][] (Thanks [@bcuff][])
* [IMPROVEMENT] New `StatsdConfig.StatsdTruncateIfTooLong` option to truncate Events and Service checks larger than 8 kB (default to True). See [#48][], [#55][]
* [IMPROVEMENT] New supported targeted frameworks: .NET Standard 1.6, .NET Core Application 1.1, .NET Core Application 2.0, .NET Framework 4.6.1. See [#52][] (Thanks [@pdpurcell][])

# 3.0.0 / 10-31-2016

## .NET Core support, end of .NET Framework 3.5 compatibility

DogStatsD-CSharp-Client `2.2.1` is the last version to support .NET Framework 3.5. As of `3.0.0`, DogStatsD-CSharp-Client supports the following platforms:
* .NET Framework 4.5.1
* .NET Standard 1.3

## Changes

* [IMPROVEMENT] Move to .NET Core, and drop .NET Framework 3.5 compatibility. See [#28][], [#39][] (Thanks [@wjdavis5][])
* [IMPROVEMENT] Abstract DogStatsD service. See [#30][], [#40][] (Thanks [@nrjohnstone][])

# 2.2.1 / 10-13-2016
* [BUGFIX] Remove the `TRACE` directive from release builds. See [#33][], [#34][] (Thanks [@albertofem][])
* [FEATURE] Service check support. See [#29][] (Thanks [@nathanrobb][])

# 2.2.0 / 08-08-2016
* [BUGFIX] Fix `Random` generator thread safety. See [#26][] (Thanks [@windsnow98][])

#  2.1.1 / 12-04-2015
* [BUGFIX] Optional automatic truncation of events that exceed the message length limit. See [#22][] (Thanks [@daniel-chambers][])

#  2.1.0 / 09-01-2015
* [BUGFIX][IMPROVEMENT] Fix `DogStatsd` unsafe thread operations. See [#18][] (Thanks [@yori-s][])

#  2.0.3 / 08-17-2015
* [BUGFIX] Fix event's text escape when it contains windows carriage returns. See [#15][] (Thanks [@anthonychu][]

# 2.0.2 / 03-09-2015
* [IMPROVEMENT] Strong-name-assembly. See [#11][]

# 2.0.1 / 02-10-2015
* [BUGFIX] Remove NUnit dependency from StatsdClient project. See [#8][] (Thanks [@michaellockwood][])

# 2.0.0 / 01-21-2015
* [FEATURE] Event support
* [FEATURE] Increment/decrement by value.
* [IMPROVEMENT] UDP packets UTF-8 encoding (was ASCII).

# 1.1.0 / 07-17-2013
* [IMPROVEMENT] UDP packets containing multiple metrics that are over the UDP packet size limit will now be split into multiple appropriately-sized packets if possible.

# 1.0.0 / 07-02-2013
* Initial release

<!--- The following link definition list is generated by PimpMyChangelog --->
[#3]: https://github.com/DataDog/dogstatsd-csharp-client/issues/3
[#8]: https://github.com/DataDog/dogstatsd-csharp-client/issues/8
[#11]: https://github.com/DataDog/dogstatsd-csharp-client/issues/11
[#15]: https://github.com/DataDog/dogstatsd-csharp-client/issues/15
[#18]: https://github.com/DataDog/dogstatsd-csharp-client/issues/18
[#22]: https://github.com/DataDog/dogstatsd-csharp-client/issues/22
[#26]: https://github.com/DataDog/dogstatsd-csharp-client/issues/26
[#28]: https://github.com/DataDog/dogstatsd-csharp-client/issues/28
[#29]: https://github.com/DataDog/dogstatsd-csharp-client/issues/29
[#30]: https://github.com/DataDog/dogstatsd-csharp-client/issues/30
[#33]: https://github.com/DataDog/dogstatsd-csharp-client/issues/33
[#34]: https://github.com/DataDog/dogstatsd-csharp-client/issues/34
[#39]: https://github.com/DataDog/dogstatsd-csharp-client/issues/39
[#40]: https://github.com/DataDog/dogstatsd-csharp-client/issues/40
[#43]: https://github.com/DataDog/dogstatsd-csharp-client/issues/43
[#44]: https://github.com/DataDog/dogstatsd-csharp-client/issues/44
[#48]: https://github.com/DataDog/dogstatsd-csharp-client/issues/48
[#50]: https://github.com/DataDog/dogstatsd-csharp-client/issues/50
[#52]: https://github.com/DataDog/dogstatsd-csharp-client/issues/52
[#54]: https://github.com/DataDog/dogstatsd-csharp-client/issues/54
[#55]: https://github.com/DataDog/dogstatsd-csharp-client/issues/55
[#59]: https://github.com/DataDog/dogstatsd-csharp-client/issues/59
[#60]: https://github.com/DataDog/dogstatsd-csharp-client/issues/60
[#61]: https://github.com/DataDog/dogstatsd-csharp-client/issues/61
[#62]: https://github.com/DataDog/dogstatsd-csharp-client/issues/62
[#63]: https://github.com/DataDog/dogstatsd-csharp-client/issues/63
[#65]: https://github.com/DataDog/dogstatsd-csharp-client/issues/65
[#66]: https://github.com/DataDog/dogstatsd-csharp-client/issues/66
[#78]: https://github.com/DataDog/dogstatsd-csharp-client/issues/78
[#92]: https://github.com/DataDog/dogstatsd-csharp-client/issues/92
[#96]: https://github.com/DataDog/dogstatsd-csharp-client/issues/96
[#108]: https://github.com/DataDog/dogstatsd-csharp-client/issues/108
[#109]: https://github.com/DataDog/dogstatsd-csharp-client/issues/109
[#110]: https://github.com/DataDog/dogstatsd-csharp-client/issues/110
[#111]: https://github.com/DataDog/dogstatsd-csharp-client/issues/111
[#112]: https://github.com/DataDog/dogstatsd-csharp-client/issues/112
[#113]: https://github.com/DataDog/dogstatsd-csharp-client/issues/113
[#114]: https://github.com/DataDog/dogstatsd-csharp-client/issues/114
[#120]: https://github.com/DataDog/dogstatsd-csharp-client/issues/120
[#121]: https://github.com/DataDog/dogstatsd-csharp-client/issues/121
[#123]: https://github.com/DataDog/dogstatsd-csharp-client/issues/123
[#124]: https://github.com/DataDog/dogstatsd-csharp-client/issues/124
[#125]: https://github.com/DataDog/dogstatsd-csharp-client/issues/125
[#127]: https://github.com/DataDog/dogstatsd-csharp-client/issues/127
[#128]: https://github.com/DataDog/dogstatsd-csharp-client/issues/128
[#130]: https://github.com/DataDog/dogstatsd-csharp-client/issues/130
[#132]: https://github.com/DataDog/dogstatsd-csharp-client/issues/132
[#133]: https://github.com/DataDog/dogstatsd-csharp-client/issues/133
[#134]: https://github.com/DataDog/dogstatsd-csharp-client/issues/134
[#137]: https://github.com/DataDog/dogstatsd-csharp-client/issues/137
[#139]: https://github.com/DataDog/dogstatsd-csharp-client/issues/139
[#143]: https://github.com/DataDog/dogstatsd-csharp-client/issues/143
[#144]: https://github.com/DataDog/dogstatsd-csharp-client/issues/144
[#151]: https://github.com/DataDog/dogstatsd-csharp-client/issues/151
[#155]: https://github.com/DataDog/dogstatsd-csharp-client/issues/155
[#159]: https://github.com/DataDog/dogstatsd-csharp-client/issues/159
[#160]: https://github.com/DataDog/dogstatsd-csharp-client/issues/160
[#161]: https://github.com/DataDog/dogstatsd-csharp-client/issues/161
[#162]: https://github.com/DataDog/dogstatsd-csharp-client/issues/162
[#163]: https://github.com/DataDog/dogstatsd-csharp-client/issues/163
[#164]: https://github.com/DataDog/dogstatsd-csharp-client/issues/164
[#165]: https://github.com/DataDog/dogstatsd-csharp-client/issues/165
[#166]: https://github.com/DataDog/dogstatsd-csharp-client/issues/166
[#167]: https://github.com/DataDog/dogstatsd-csharp-client/issues/167
[#168]: https://github.com/DataDog/dogstatsd-csharp-client/issues/168
[#169]: https://github.com/DataDog/dogstatsd-csharp-client/issues/169
[#170]: https://github.com/DataDog/dogstatsd-csharp-client/issues/170
[#171]: https://github.com/DataDog/dogstatsd-csharp-client/issues/171
[#172]: https://github.com/DataDog/dogstatsd-csharp-client/issues/172
[@DanielVukelich]: https://github.com/DanielVukelich
[@albertofem]: https://github.com/albertofem
[@alistair]: https://github.com/alistair
[@anthonychu]: https://github.com/anthonychu
[@bcuff]: https://github.com/bcuff
[@carlreid]: https://github.com/carlreid
[@chriskinsman]: https://github.com/chriskinsman
[@daniel-chambers]: https://github.com/daniel-chambers
[@danopia]: https://github.com/danopia
[@fjmorel]: https://github.com/fjmorel
[@jdasilva-olo]: https://github.com/jdasilva-olo
[@jpasichnyk]: https://github.com/jpasichnyk
[@kevingosse]: https://github.com/kevingosse
[@michaellockwood]: https://github.com/michaellockwood
[@nathanrobb]: https://github.com/nathanrobb
[@nrjohnstone]: https://github.com/nrjohnstone
[@pdpurcell]: https://github.com/pdpurcell
[@shreyamsh]: https://github.com/shreyamsh
[@sqdk]: https://github.com/sqdk
[@windsnow98]: https://github.com/windsnow98
[@wjdavis5]: https://github.com/wjdavis5
[@yori-s]: https://github.com/yori-s
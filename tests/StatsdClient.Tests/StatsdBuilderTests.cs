using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mono.Unix;
using Moq;
using NUnit.Framework;
using StatsdClient.Aggregator;
using StatsdClient.Bufferize;
using StatsdClient.Transport;

namespace StatsdClient.Tests
{
    [TestFixture]
    public class StatsdBuilderTests
    {
        private readonly Dictionary<string, string> _envVarsToRestore = new Dictionary<string, string>();
        private readonly List<string> _envVarsKeyToRestore = new List<string>
        {
            StatsdConfig.DogStatsdPortEnvVar,
            StatsdConfig.AgentHostEnvVar,
            StatsdConfig.EntityIdEnvVar,
            StatsdConfig.AgentPipeNameEnvVar,
            StatsdConfig.EnvironmentEnvVar,
            StatsdConfig.ServiceEnvVar,
            StatsdConfig.VersionEnvVar,
        };

        private Mock<IStatsBufferizeFactory> _mock;
        private StatsdBuilder _statsdBuilder;
        private UnixEndPoint _unixEndPoint;
        private IPEndPoint _ipEndPoint;

        [SetUp]
        public void Init()
        {
            _mock = new Mock<IStatsBufferizeFactory>(MockBehavior.Loose);
            _statsdBuilder = new StatsdBuilder(_mock.Object);
            _mock.Setup(m => m.CreateUnixDomainSocketTransport(
                            It.IsAny<UnixEndPoint>(),
                            It.IsAny<TimeSpan?>()))
                            .Returns<UnixEndPoint, TimeSpan?>((e, d) =>
                            {
                                _unixEndPoint = e;
                                return new UnixDomainSocketTransport(e, d);
                            });
            _unixEndPoint = null;

            _mock.Setup(m => m.CreateUDPTransport(It.IsAny<IPEndPoint>()))
                        .Returns<IPEndPoint>(e =>
                        {
                            _ipEndPoint = e;
                            return new UDPTransport(e);
                        });
            _ipEndPoint = null;

            foreach (var key in _envVarsKeyToRestore)
            {
                _envVarsToRestore[key] = Environment.GetEnvironmentVariable(key);
            }

            // Set default hostname
            Environment.SetEnvironmentVariable(StatsdConfig.AgentHostEnvVar, "0.0.0.0");
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var env in _envVarsToRestore)
            {
                Environment.SetEnvironmentVariable(env.Key, env.Value);
            }
        }

        [Test]
        public void StatsdServerName()
        {
            Environment.SetEnvironmentVariable(StatsdConfig.AgentHostEnvVar, null);
            Assert.Throws<ArgumentNullException>(() => GetStatsdServerName(CreateConfig()));

            Assert.AreEqual("0.0.0.1", GetStatsdServerName(CreateConfig(statsdServerName: "0.0.0.1")));

            Environment.SetEnvironmentVariable(StatsdConfig.AgentHostEnvVar, "0.0.0.2");
            Assert.AreEqual("0.0.0.2", GetStatsdServerName(CreateConfig()));

            Assert.AreEqual("0.0.0.3", GetStatsdServerName(CreateConfig(statsdServerName: "0.0.0.3")));
        }

        [Test]
        public void UDPPort()
        {
            Assert.AreEqual(StatsdConfig.DefaultStatsdPort, GetUDPPort(CreateConfig()));

            Assert.AreEqual(1, GetUDPPort(CreateConfig(statsdPort: 1)));

            Environment.SetEnvironmentVariable(StatsdConfig.DogStatsdPortEnvVar, "2");
            Assert.AreEqual(2, GetUDPPort(CreateConfig()));

            Assert.AreEqual(3, GetUDPPort(CreateConfig(statsdPort: 3)));
        }

#if !OS_WINDOWS
        [Test]
        public void UDSStatsdServerName()
        {
            Environment.SetEnvironmentVariable(StatsdConfig.AgentHostEnvVar, null);
            Assert.AreEqual("server1", GetUDSStatsdServerName(CreateUDSConfig("server1")));

            Environment.SetEnvironmentVariable(
                StatsdConfig.AgentHostEnvVar,
                StatsdBuilder.UnixDomainSocketPrefix + "server2");
            Assert.AreEqual("server2", GetUDSStatsdServerName(CreateUDSConfig()));

            Assert.AreEqual("server3", GetUDSStatsdServerName(CreateUDSConfig("server3")));
        }
#endif

        [Test]
        public void CreateStatsBufferizeUDP()
        {
            var config = new StatsdConfig { };
            var conf = config.Advanced;

            conf.TelemetryFlushInterval = null;
            config.StatsdMaxUDPPacketSize = 10;
            conf.MaxMetricsInAsyncQueue = 2;
            conf.MaxBlockDuration = TimeSpan.FromMilliseconds(3);
            conf.DurationBeforeSendingNotFullBuffer = TimeSpan.FromMilliseconds(4);

            BuildStatsData(config);
            _mock.Verify(m => m.CreateStatsBufferize(
                It.IsAny<StatsRouter>(),
                conf.MaxMetricsInAsyncQueue,
                conf.MaxBlockDuration,
                conf.DurationBeforeSendingNotFullBuffer));
            _mock.Verify(
                m => m.CreateStatsRouter(
                It.IsAny<Serializers>(),
                It.Is<BufferBuilder>(b => b.Capacity == config.StatsdMaxUDPPacketSize),
                It.IsAny<Aggregators>()));
        }

#if !OS_WINDOWS
        [Test]
        public void CreateStatsBufferizeUDS()
        {
            var config = CreateUDSConfig("server1");
            config.StatsdMaxUnixDomainSocketPacketSize = 20;

            BuildStatsData(config);
            _mock.Verify(m => m.CreateStatsBufferize(
                It.IsAny<StatsRouter>(),
                It.IsAny<int>(),
                null,
                It.IsAny<TimeSpan>()));
            _mock.Verify(
                m => m.CreateStatsRouter(
                It.IsAny<Serializers>(),
                It.Is<BufferBuilder>(b => b.Capacity == config.StatsdMaxUnixDomainSocketPacketSize),
                It.IsAny<Aggregators>()));
        }
#endif

        [Test]
        public void CreateTelemetry()
        {
            Environment.SetEnvironmentVariable(StatsdConfig.EntityIdEnvVar, "EntityId");
            Environment.SetEnvironmentVariable(StatsdConfig.ServiceEnvVar, "service");
            Environment.SetEnvironmentVariable(StatsdConfig.EnvironmentEnvVar, "env");
            Environment.SetEnvironmentVariable(StatsdConfig.VersionEnvVar, "version");

            var config = new StatsdConfig { };
            var conf = config.Advanced;

            conf.TelemetryFlushInterval = TimeSpan.FromMinutes(1);
            config.ConstantTags = new[] { "key:value" };

            var expectedTags = new List<string>(config.ConstantTags);
            expectedTags.Add("dd.internal.entity_id:EntityId");
            expectedTags.Add($"{StatsdConfig.ServiceTagKey}:service");
            expectedTags.Add($"{StatsdConfig.EnvironmentTagKey}:env");
            expectedTags.Add($"{StatsdConfig.VersionTagKey}:version");

            BuildStatsData(config);
            _mock.Verify(m => m.CreateUDPTransport(It.IsAny<IPEndPoint>()), Times.Once);
            _mock.Verify(
                m => m.CreateTelemetry(
                It.IsAny<MetricSerializer>(),
                It.Is<string>(v => !string.IsNullOrEmpty(v)),
                conf.TelemetryFlushInterval.Value,
                It.IsAny<ITransport>(),
                It.Is<string[]>(tags => Enumerable.SequenceEqual(tags, expectedTags))));
        }

        [Test]
        public void TelemetryEndPoint()
        {
            var config = new StatsdConfig { };
            var conf = config.Advanced;
            conf.OptionalTelemetryEndPoint = new DogStatsdEndPoint { ServerName = "0.0.0.1", Port = 42 };

            BuildStatsData(config);
            _mock.Verify(m => m.CreateUDPTransport(It.IsAny<IPEndPoint>()), Times.Exactly(2));
        }

        [Test]
        public void ClientSideAggregation()
        {
            var config = new StatsdConfig { };
            config.ClientSideAggregation = null;
            BuildStatsData(config);
            _mock.Verify(
                m => m.CreateStatsRouter(
                It.IsAny<Serializers>(),
                It.Is<BufferBuilder>(b => b.Capacity == config.StatsdMaxUDPPacketSize),
                null));

            config.ClientSideAggregation = new ClientSideAggregationConfig();
            BuildStatsData(config);
            _mock.Verify(
                m => m.CreateStatsRouter(
                It.IsAny<Serializers>(),
                It.Is<BufferBuilder>(b => b.Capacity == config.StatsdMaxUDPPacketSize),
                It.IsNotNull<Aggregators>()));
        }

        [Test]
        public void PipeName()
        {
            var config = new StatsdConfig { };
            config.StatsdServerName = string.Empty;
            Environment.SetEnvironmentVariable(StatsdConfig.AgentHostEnvVar, string.Empty);

            _mock.Setup(m => m.CreateNamedPipeTransport(It.IsAny<string>())).Returns(new NamedPipeTransport("pipename"));

            Environment.SetEnvironmentVariable(StatsdConfig.AgentPipeNameEnvVar, "TestEnv");
            BuildStatsData(config);
            _mock.Verify(m => m.CreateNamedPipeTransport("TestEnv"));

            config.PipeName = "Test";
            BuildStatsData(config);
            _mock.Verify(m => m.CreateNamedPipeTransport(config.PipeName));
        }

        private static StatsdConfig CreateUDSConfig(string server = null)
        {
            var config = new StatsdConfig();
            if (server != null)
            {
                config.StatsdServerName = StatsdBuilder.UnixDomainSocketPrefix + server;
            }

            config.Advanced.TelemetryFlushInterval = null;
            return config;
        }

        private static StatsdConfig CreateConfig(string statsdServerName = null, int? statsdPort = null)
        {
            var config = new StatsdConfig { StatsdServerName = statsdServerName };
            if (statsdPort.HasValue)
            {
                config.StatsdPort = statsdPort.Value;
            }

            config.Advanced.TelemetryFlushInterval = null;
            return config;
        }

        private int GetUDPPort(StatsdConfig config)
        {
            var endPoint = GetUDPIPEndPoint(config);
            return endPoint.Port;
        }

        private string GetStatsdServerName(StatsdConfig config)
        {
            var endPoint = GetUDPIPEndPoint(config);
            return endPoint.Address.ToString();
        }

        private string GetUDSStatsdServerName(StatsdConfig config)
        {
            BuildStatsData(config);
            Assert.NotNull(_unixEndPoint);

            return _unixEndPoint.Filename;
        }

        private IPEndPoint GetUDPIPEndPoint(StatsdConfig config)
        {
            BuildStatsData(config);
            Assert.NotNull(_ipEndPoint);

            return _ipEndPoint;
        }

        private void BuildStatsData(StatsdConfig config)
        {
            var buildStatsData = _statsdBuilder.BuildStatsData(config);
            buildStatsData.Dispose();
        }
    }
}
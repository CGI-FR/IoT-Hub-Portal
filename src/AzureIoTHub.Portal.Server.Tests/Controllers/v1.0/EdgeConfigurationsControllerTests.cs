using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Interfaces;
using AzureIoTHub.Portal.Server.Services;
using Microsoft.Azure.Devices;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
    [TestFixture]
    public class EdgeConfigurationsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IConfigs> mockConfigsServices;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfigsServices = this.mockRepository.Create<IConfigs>();
        }

        private EdgeConfigurationsController CreateEdgeConfigurationsController()
        {
            return new EdgeConfigurationsController(
                this.mockConfigsServices.Object);
        }

        [Test]
        public async Task Get_AllConfiguration_Should_Return_Config_ListItems()
        {
            // Arrange
            var expected = GetModuleConfiguration(3).ToArray();

            var edgeConfigurationsController = this.CreateEdgeConfigurationsController();
            this.mockConfigsServices.Setup(c => c.GetAllConfigs())
                .ReturnsAsync(expected);

            // Act
            var result = await edgeConfigurationsController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Count(), result.Count());

            for (int i = 0; i < expected.Count(); i++)
            {
                var expectedItem = expected.ElementAt(i);
                var resultItem = result.ElementAt(i);

                Assert.AreEqual(expectedItem.Id, resultItem.ConfigurationID);
                Assert.AreEqual(expectedItem.TargetCondition, resultItem.Conditions);
                Assert.AreEqual(expectedItem.Priority, resultItem.Priority);

                Assert.AreEqual(expectedItem.SystemMetrics.Results["targetedCount"], resultItem.MetricsTargeted);
                Assert.AreEqual(expectedItem.SystemMetrics.Results["appliedCount"], resultItem.MetricsApplied);
                Assert.AreEqual(expectedItem.SystemMetrics.Results["reportedSuccessfulCount"], resultItem.MetricsSuccess);
                Assert.AreEqual(expectedItem.SystemMetrics.Results["reportedFailedCount"], resultItem.MetricsFailure);

                Assert.AreEqual(1, resultItem.Modules.Count());

            }

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Get_Configuration_Should_Return_The_Configuration()
        {
            // Arrange
            var expected = GetModuleConfiguration(1).Single();

            var edgeConfigurationsController = this.CreateEdgeConfigurationsController();
            this.mockConfigsServices.Setup(c => c.GetConfigItem(expected.Id))
                .ReturnsAsync(expected);

            // Act
            var result = await edgeConfigurationsController.Get(expected.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Id, result.ConfigurationID);
            Assert.AreEqual(expected.TargetCondition, result.Conditions);
            Assert.AreEqual(expected.Priority, result.Priority);

            Assert.AreEqual(expected.SystemMetrics.Results["targetedCount"], result.MetricsTargeted);
            Assert.AreEqual(expected.SystemMetrics.Results["appliedCount"], result.MetricsApplied);
            Assert.AreEqual(expected.SystemMetrics.Results["reportedSuccessfulCount"], result.MetricsSuccess);
            Assert.AreEqual(expected.SystemMetrics.Results["reportedFailedCount"], result.MetricsFailure);

            Assert.AreEqual(4, result.Modules.Count());

            var firstModule = result.Modules.First();
            Assert.IsNotNull(firstModule);

            Assert.AreEqual(4, firstModule.EnvironmentVariables.Count());
            Assert.AreEqual("LoRaWanNetworkSrvModule", firstModule.ModuleName);
            Assert.AreEqual("running", firstModule.Status);
            Assert.AreEqual("loraedge/lorawannetworksrvmodule:2.0.0", firstModule.Version);

            this.mockRepository.VerifyAll();
        }

        private IEnumerable<Configuration> GetModuleConfiguration(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var configuration = new Configuration(Guid.NewGuid().ToString())
                {
                    Content = new ConfigurationContent(),
                };

                configuration.SystemMetrics.Results.TryAdd("targetedCount", Random.Shared.Next());
                configuration.SystemMetrics.Results.TryAdd("appliedCount", Random.Shared.Next());
                configuration.SystemMetrics.Results.TryAdd("reportedSuccessfulCount", Random.Shared.Next());
                configuration.SystemMetrics.Results.TryAdd("reportedFailedCount", Random.Shared.Next());

                configuration.Priority = Random.Shared.Next();
                configuration.TargetCondition = Guid.NewGuid().ToString();

                var edgeAgentModule = new Dictionary<string, object>();
                var edgeAgentProperties = JObject.Parse("{\"modules\":{\"LoRaWanNetworkSrvModule\":{\"settings\":{\"image\":\"loraedge/lorawannetworksrvmodule:2.0.0\",\"createOptions\":\"{\\\"ExposedPorts\\\":{\\\"5000/tcp\\\":{}},\\\"HostConfig\\\":{\\\"PortBindings\\\":{\\\"5000/tcp\\\":[{\\\"HostPort\\\":\\\"5000\\\",\\\"HostIp\\\":\\\"172.17.0.1\\\"}]}}}\"},\"type\":\"docker\",\"version\":\"1.0\",\"env\":{\"LOG_TO_HUB\":{\"value\":\"false\"},\"LOG_LEVEL\":{\"value\":\"1\"},\"LOG_TO_CONSOLE\":{\"value\":\"true\"},\"ENABLE_GATEWAY\":{\"value\":\"true\"}},\"status\":\"running\",\"restartPolicy\":\"always\"},\"LoRaBasicsStationModule\":{\"settings\":{\"image\":\"loraedge/lorabasicsstationmodule:2.0.0\",\"createOptions\":\"{\\\"HostConfig\\\":{\\\"NetworkMode\\\":\\\"host\\\",\\\"Privileged\\\":true},\\\"NetworkingConfig\\\":{\\\"EndpointsConfig\\\":{\\\"host\\\":{}}}}\"},\"type\":\"docker\",\"env\":{\"RESET_PIN\":{\"value\":\"2\"},\"TC_URI\":{\"value\":\"ws://172.17.0.1:5000\"}},\"status\":\"running\",\"restartPolicy\":\"always\",\"version\":\"1.0\"}},\"runtime\":{\"settings\":{\"minDockerVersion\":\"v1.25\"},\"type\":\"docker\"},\"schemaVersion\":\"1.0\",\"systemModules\":{\"edgeAgent\":{\"settings\":{\"image\":\"mcr.microsoft.com/azureiotedge-agent:1.1.8\",\"createOptions\":\"\"},\"type\":\"docker\",\"env\":{\"UpstreamProtocol\":{\"value\":\"AmqpWs\"},\"SendRuntimeQualityTelemetry\":{\"value\":\"false\"}}},\"edgeHub\":{\"settings\":{\"image\":\"mcr.microsoft.com/azureiotedge-hub:1.1.8\",\"createOptions\":\"\"},\"type\":\"docker\",\"env\":{\"RuntimeLogLevel\":{\"value\":\"warning\"},\"httpSettings__enabled\":{\"value\":\"true\"},\"UpstreamProtocol\":{\"value\":\"AmqpWs\"},\"NestedEdgeEnabled\":{\"value\":\"false\"},\"OptimizeForPerformance\":{\"value\":\"false\"},\"TwinManagerVersion\":{\"value\":\"v2\"},\"mqttSettings__enabled\":{\"value\":\"false\"},\"AuthenticationMode\":{\"value\":\"CloudAndScope\"}},\"status\":\"running\",\"restartPolicy\":\"always\"}}}");

                edgeAgentModule.Add("properties.desired", edgeAgentProperties);

                configuration.Content.ModulesContent.Add("$edgeAgent", edgeAgentModule);

                yield return configuration;
            }
        }
    }
}

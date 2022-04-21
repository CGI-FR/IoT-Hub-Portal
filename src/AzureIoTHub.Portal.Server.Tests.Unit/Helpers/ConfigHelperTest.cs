// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Helpers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Helpers;
    using Microsoft.Azure.Devices;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigHelperTest
    {
        //[Test]
        //[TestCase("targetedCount")]
        //[TestCase("appliedCount")]
        //public void RetrieveMetricValue(string metricName)
        //{
        //    //Arrange
        //    var metricValues = new Dictionary<string, long>
        //    {
        //        { "targetedCount", 2 },
        //        { "appliedCount", 2 }
        //    };

        //    var configMetric = new ConfigurationMetrics(){ Results = metricValues };

        //    //var config = new Configuration("test"){ SystemMetrics = configMetric };

        //    var mockConfig = new Mock<Configuration>(MockBehavior.Strict);
        //    mockConfig.SetupProperty(x => x.SystemMetrics).SetReturnsDefault(configMetric);

        //    // Act
        //    var result = ConfigHelper.RetrieveMetricValue(mockConfig.Object, metricName);

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(2, result);
        //}

        [Test]
        public void CreateDeviceConfigShouldReturnValue()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();
            var targetCondition = $"tags.modelId = '{modelId}' and tags.name = test and tags.name01 = test";

            var config = new Configuration("test")
            {
                TargetCondition = targetCondition,
                Labels = new Dictionary<string, string> { { "id", "test" } },
                Priority = 1
            };

            // Act
            var result = ConfigHelper.CreateDeviceConfig(config);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(config.Priority, result.Priority);
            Assert.AreEqual(modelId, result.ModelId);
        }

        [Test]
        public void CreateDeviceConfigWithNullArgumentShouldThrowException()
        {
            // Arrange

            // Act

            // Assert
            var argumentNullException = Assert.Throws<ArgumentNullException>(() => ConfigHelper.CreateDeviceConfig(config: null));
            Assert.IsNotNull(argumentNullException);
            Assert.AreEqual("config", argumentNullException.ParamName);
            Assert.IsInstanceOf<ArgumentNullException>(argumentNullException);
        }

        [Test]
        public void CreateDeviceConfigWithNullTargetConditionShouldShouldThrowAnException()
        {
            // Arrange
            var config = new Configuration("test")
            {
                TargetCondition = string.Empty,
                Labels = new Dictionary<string, string> { { "id", "test" } },
                Priority = 1
            };

            // Act

            // Assert
            var invalidOperationException =  Assert.Throws<InvalidOperationException>(() => ConfigHelper.CreateDeviceConfig(config));
            Assert.IsNotNull(invalidOperationException);
            Assert.IsInstanceOf<InvalidOperationException>(invalidOperationException);
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Helpers
{
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Helpers;
    using Microsoft.Azure.Devices;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigHelperTest
    {
        [Test]
        [TestCase("targetedCount")]
        [TestCase("appliedCount")]
        public void RetrieveMetricValue(string metricName)
        {
            //Arrange
            var metricValues = new Dictionary<string, long>
            {
                { "targetedCount", 2 },
                { "appliedCount", 2 }
            };

            var configMetric = new ConfigurationMetrics(){ Results = metricValues };

            //var config = new Configuration("test"){ SystemMetrics = configMetric };

            var mockConfig = new Mock<Configuration>(MockBehavior.Strict);
            mockConfig.SetupProperty(x => x.SystemMetrics).SetReturnsDefault(configMetric);

            // Act
            var result = ConfigHelper.RetrieveMetricValue(mockConfig.Object, metricName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result);
        }
    }
}

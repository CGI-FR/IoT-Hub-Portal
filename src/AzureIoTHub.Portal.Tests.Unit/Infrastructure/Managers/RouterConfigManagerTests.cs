// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Managers
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Managers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class RouterConfigManagerTests : IDisposable
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private RouterConfigManager CreateManager()
        {
            return new RouterConfigManager();
        }

        [TestCase("CN_470_510_RP1")]
        [TestCase("CN_470_510_RP2")]
        [TestCase("EU_863_870")]
        [TestCase("US_902_928_FSB_1")]
        [TestCase("US_902_928_FSB_2")]
        [TestCase("US_902_928_FSB_3")]
        [TestCase("US_902_928_FSB_4")]
        [TestCase("US_902_928_FSB_5")]
        [TestCase("US_902_928_FSB_6")]
        [TestCase("US_902_928_FSB_7")]
        [TestCase("US_902_928_FSB_8")]
        public async Task GetRouterConfigStateUnderTestExpectedBehavior(string loraRegion)
        {
            // Arrange
            var routerConfig = CreateManager();

            // Act
            var result = await routerConfig.GetRouterConfig(loraRegion);

            // Assert
            Assert.IsNotNull(result);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}

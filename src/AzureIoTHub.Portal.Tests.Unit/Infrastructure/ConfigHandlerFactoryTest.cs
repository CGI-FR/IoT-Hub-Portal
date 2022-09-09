// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure
{
    using AzureIoTHub.Portal.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigHandlerFactoryTest
    {
        private MockRepository mockRepository;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IHostEnvironment> mockHostEnvironment;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.mockHostEnvironment = this.mockRepository.Create<IHostEnvironment>();
        }

        [Test]
        public void WhenUsingDevEnvironmentShouldReturnDevelopmentConfigHandler()
        {
            // Arrange
            _ = this.mockHostEnvironment.Setup(c => c.EnvironmentName)
                .Returns(Environments.Development);

            // Act
            var result = ConfigHandlerFactory.Create(this.mockHostEnvironment.Object, this.mockConfiguration.Object);

            // Assert
            Assert.IsAssignableFrom<DevelopmentConfigHandler>(result);
        }


        [Test]
        public void WhenUsingProdEnvironmentShouldReturnProductionConfigHandler()
        {
            // Arrange
            _ = this.mockHostEnvironment.Setup(c => c.EnvironmentName)
                    .Returns(Environments.Production);

            // Act
            var result = ConfigHandlerFactory.Create(this.mockHostEnvironment.Object, this.mockConfiguration.Object);

            // Assert
            Assert.IsAssignableFrom<ProductionConfigHandler>(result);
        }
    }
}

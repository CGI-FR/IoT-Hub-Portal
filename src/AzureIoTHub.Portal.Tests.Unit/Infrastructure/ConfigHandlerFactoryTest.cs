// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure
{
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Shared.Constants;
    using AzureIoTHub.Portal.Shared.Constants;
    using AzureIoTHub.Portal.Infrastructure;
    using FluentAssertions;
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
            _ = this.mockConfiguration.Setup(cnf => cnf[ConfigHandlerBase.CloudProviderKey])
                    .Returns(CloudProviders.Azure);

            // Act
            var result = ConfigHandlerFactory.Create(this.mockHostEnvironment.Object, this.mockConfiguration.Object);

            // Assert
            Assert.IsAssignableFrom<DevelopmentConfigHandler>(result);
        }


        [Test]
        public void WhenUsingProdAzureEnvironmentShouldReturnProductionAzureConfigHandler()
        {
            // Arrange
            _ = this.mockHostEnvironment.Setup(c => c.EnvironmentName)
                    .Returns(Environments.Production);
            _ = this.mockConfiguration.Setup(cnf => cnf[ConfigHandlerBase.CloudProviderKey])
                    .Returns(CloudProviders.Azure);

            // Act
            var result = ConfigHandlerFactory.Create(this.mockHostEnvironment.Object, this.mockConfiguration.Object);

            // Assert
            Assert.IsAssignableFrom<ProductionAzureConfigHandler>(result);
        }

        [Test]
        public void WhenUsingProdAWSEnvironmentShouldReturnProductionAWSConfigHandler()
        {
            // Arrange
            _ = this.mockHostEnvironment.Setup(c => c.EnvironmentName)
                    .Returns(Environments.Production);
            _ = this.mockConfiguration.Setup(cnf => cnf[ConfigHandlerBase.CloudProviderKey])
                    .Returns(CloudProviders.AWS);

            // Act
            var result = ConfigHandlerFactory.Create(this.mockHostEnvironment.Object, this.mockConfiguration.Object);

            // Assert
            Assert.IsAssignableFrom<ProductionAWSConfigHandler>(result);
        }

        [Test]
        public void WhenNoConfigCloudProviderShouldThrowInvalidCloudProviderException()
        {
            // Arrange
            _ = this.mockHostEnvironment.Setup(c => c.EnvironmentName)
                    .Returns(Environments.Production);
            _ = this.mockConfiguration.Setup(cnf => cnf[ConfigHandlerBase.CloudProviderKey])
                    .Returns((string)null);

            // Act
            var result = () => ConfigHandlerFactory.Create(this.mockHostEnvironment.Object, this.mockConfiguration.Object);

            // Assert
            _ = result.Should().Throw<InvalidCloudProviderException>().WithMessage(ErrorTitles.InvalidCloudProviderUndefined);
        }

        [Test]
        public void WhenWrongCloudProviderShouldThrowInvalidCloudProviderException()
        {
            // Arrange
            _ = this.mockHostEnvironment.Setup(c => c.EnvironmentName)
                    .Returns(Environments.Production);
            _ = this.mockConfiguration.Setup(cnf => cnf[ConfigHandlerBase.CloudProviderKey])
                    .Returns("Test");

            // Act
            var result = () => ConfigHandlerFactory.Create(this.mockHostEnvironment.Object, this.mockConfiguration.Object);

            // Assert
            _ = result.Should().Throw<InvalidCloudProviderException>().WithMessage(ErrorTitles.InvalidCloudProviderIncorrect);
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
{
    using System;
    using Azure.Data.Tables;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Infrastructure.Mappers;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class LoRaDeviceModelMapperTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
        }

        private LoRaDeviceModelMapper CreateLoRaDeviceModelMapper()
        {
            return new LoRaDeviceModelMapper(
                this.mockDeviceModelImageManager.Object);
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void CreateDeviceModelListItemStateUnderTestExpectedBehavior(bool isBuiltin, bool supportLora)
        {
            // Arrange
            var loRaDeviceModelMapper = CreateLoRaDeviceModelMapper();
            var entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var expectedModelImageUri = new Uri($"https://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedModelImageUri);

            entity[nameof(LoRaDeviceModelDto.IsBuiltin)] = isBuiltin;
            entity[nameof(LoRaDeviceModelDto.SupportLoRaFeatures)] = supportLora;
            entity[nameof(LoRaDeviceModelDto.Name)] = "FAKE DEVICE";
            entity[nameof(LoRaDeviceModelDto.Description)] = "FAKE DESCRIPTION";

            // Act
            var result = loRaDeviceModelMapper.CreateDeviceModelListItem(entity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.RowKey, result.ModelId);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.Name)], result.Name);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.Description)], result.Description);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.SupportLoRaFeatures)], supportLora);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.IsBuiltin)], isBuiltin);

            this.mockRepository.VerifyAll();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void CreateDeviceModelStateUnderTestExpectedBehavior(bool isBuiltin, bool supportLora)
        {
            // Arrange
            var loRaDeviceModelMapper = CreateLoRaDeviceModelMapper();
            var entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var expectedModelImageUri = new Uri($"https://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedModelImageUri);

            entity[nameof(LoRaDeviceModelDto.IsBuiltin)] = isBuiltin;
            entity[nameof(LoRaDeviceModelDto.SupportLoRaFeatures)] = supportLora;
            entity[nameof(LoRaDeviceModelDto.Name)] = "FAKE DEVICE";
            entity[nameof(LoRaDeviceModelDto.Description)] = "FAKE DESCRIPTION";
            entity[nameof(LoRaDeviceModelDto.SensorDecoder)] = "FAKE SENSORDECODERURL";

            // Act
            var result = loRaDeviceModelMapper.CreateDeviceModel(
                entity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.RowKey, result.ModelId);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.Name)], result.Name);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.Description)], result.Description);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.SensorDecoder)], result.SensorDecoder);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.SupportLoRaFeatures)], supportLora);
            Assert.AreEqual(entity[nameof(LoRaDeviceModelDto.IsBuiltin)], isBuiltin);
            this.mockRepository.VerifyAll();
        }

        [TestCase(false)]
        [TestCase(true)]
        [TestCase(true)]
        [TestCase(false)]
        public void UpdateTableEntityStateUnderTestExpectedBehavior(bool isBuiltin)
        {
            // Arrange
            var loRaDeviceModelMapper = CreateLoRaDeviceModelMapper();
            var model = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ImageUrl = new Uri("http://fake.local"),
                SensorDecoder = Guid.NewGuid().ToString(),
                IsBuiltin = isBuiltin
            };

            // Act
            var result = loRaDeviceModelMapper.BuildDeviceModelDesiredProperties(model);

            // Assert
            _ = result.Keys.Count.Should().Be(14);

            this.mockRepository.VerifyAll();
        }
    }
}

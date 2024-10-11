// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using IoTHub.Portal.Shared.Models.v10;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Portal.Domain.Repositories;
    using UnitTests.Bases;
    using AutoFixture;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Infrastructure.Services;
    using FluentAssertions;
    using System.Linq;
    using System.Threading;
    using System;

    [TestFixture]
    public class LayerServiceTests : BackendUnitTest
    {
        private Mock<ILayerRepository> mockLayerRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private ILayerService layerService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockLayerRepository = MockRepository.Create<ILayerRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockLayerRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<ILayerService, LayerService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.layerService = Services.GetRequiredService<ILayerService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateNewEntity()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.InsertAsync(It.IsAny<Layer>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await this.layerService.CreateLayer(expectedLayerDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedLayerDto.Name, result.Name);
            Assert.AreEqual(expectedLayerDto.Father, result.Father);
            Assert.AreEqual(expectedLayerDto.Planning, result.Planning);
            Assert.AreEqual(expectedLayerDto.hasSub, result.hasSub);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateShouldThrowResourceNotFoundException()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
                .ReturnsAsync((Layer)null);

            var act = () => this.layerService.UpdateLayer(expectedLayerDto);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The layer with id {expectedLayer.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteExistigLayerSuccessfully()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
               .ReturnsAsync(expectedLayer);

            this.mockLayerRepository.Setup(repository => repository.Delete(expectedLayerDto.Id))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            await this.layerService.DeleteLayer(expectedLayerDto.Id);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteShuldThrowResourceNotFoundException()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
                .ReturnsAsync((Layer)null);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
                .ReturnsAsync((Layer)null);

            var act = () => this.layerService.DeleteLayer(expectedLayerDto.Id);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The layer with id {expectedLayer.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetLayerShouldReturnValue()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
               .ReturnsAsync(expectedLayer);

            var result = await this.layerService.GetLayer(expectedLayerDto.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedLayerDto.Name, result.Name);
            Assert.AreEqual(expectedLayerDto.Father, result.Father);
            Assert.AreEqual(expectedLayerDto.Planning, result.Planning);
            Assert.AreEqual(expectedLayerDto.hasSub, result.hasSub);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetLayerShouldThrowResourceNotFoundException()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
                .ReturnsAsync((Layer)null);

            var act = () => this.layerService.GetLayer(expectedLayerDto.Id);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The layer with id {expectedLayer.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetLayersShouldReturnLayerList()
        {
            var expectedTotalLayersCount = 50;
            var expectedDevices = Fixture.CreateMany<Layer>(expectedTotalLayersCount).ToList();

            await DbContext.AddRangeAsync(expectedDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockLayerRepository.Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDevices);

            var result = await this.layerService.GetLayers();

            _ = result.Count().Should().Be(expectedTotalLayersCount);
            MockRepository.VerifyAll();
        }
    }
}

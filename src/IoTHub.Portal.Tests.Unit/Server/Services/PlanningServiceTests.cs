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
    public class PlanningServiceTests : BackendUnitTest
    {
        private Mock<IPlanningRepository> mockPlanningRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private IPlanningService planningService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockPlanningRepository = MockRepository.Create<IPlanningRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockPlanningRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IPlanningService, PlanningService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.planningService = Services.GetRequiredService<IPlanningService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateNewEntity()
        {
            var expectedPlanning = Fixture.Create<Planning>();
            var expectedPlanningDto = Mapper.Map<PlanningDto>(expectedPlanning);

            _ = this.mockPlanningRepository.Setup(repository => repository.InsertAsync(It.IsAny<Planning>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await this.planningService.CreatePlanning(expectedPlanningDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPlanningDto.Name, result.Name);
            Assert.AreEqual(expectedPlanningDto.Start, result.Start);
            Assert.AreEqual(expectedPlanningDto.End, result.End);
            Assert.AreEqual(expectedPlanningDto.Frequency, result.Frequency);
            Assert.AreEqual(expectedPlanningDto.Day, result.Day);
            Assert.AreEqual(expectedPlanningDto.CommandId, result.CommandId);
            Assert.AreEqual(expectedPlanningDto.DayExceptions, result.DayExceptions);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateShouldThrowResourceNotFoundException()
        {
            var expectedPlanning = Fixture.Create<Planning>();
            var expectedPlanningDto = Mapper.Map<PlanningDto>(expectedPlanning);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(expectedPlanningDto.Id))
                .ReturnsAsync((Planning)null);

            var act = () => this.planningService.UpdatePlanning(expectedPlanningDto);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The planning with id {expectedPlanning.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteExistigPlanningSuccessfully()
        {
            var expectedPlanning = Fixture.Create<Planning>();
            var expectedPlanningDto = Mapper.Map<PlanningDto>(expectedPlanning);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(expectedPlanningDto.Id))
               .ReturnsAsync(expectedPlanning);

            this.mockPlanningRepository.Setup(repository => repository.Delete(expectedPlanningDto.Id))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            await this.planningService.DeletePlanning(expectedPlanningDto.Id);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteShuldThrowResourceNotFoundException()
        {
            var expectedPlanning = Fixture.Create<Planning>();
            var expectedPlanningDto = Mapper.Map<PlanningDto>(expectedPlanning);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(expectedPlanningDto.Id))
                .ReturnsAsync((Planning)null);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(expectedPlanningDto.Id))
                .ReturnsAsync((Planning)null);

            var act = () => this.planningService.DeletePlanning(expectedPlanningDto.Id);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The planning with id {expectedPlanning.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPlanningShouldReturnValue()
        {
            var expectedPlanning = Fixture.Create<Planning>();
            var expectedPlanningDto = Mapper.Map<PlanningDto>(expectedPlanning);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(expectedPlanningDto.Id))
               .ReturnsAsync(expectedPlanning);

            var result = await this.planningService.GetPlanning(expectedPlanningDto.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPlanningDto.Name, result.Name);
            Assert.AreEqual(expectedPlanningDto.Start, result.Start);
            Assert.AreEqual(expectedPlanningDto.End, result.End);
            Assert.AreEqual(expectedPlanningDto.Frequency, result.Frequency);
            Assert.AreEqual(expectedPlanningDto.Day, result.Day);
            Assert.AreEqual(expectedPlanningDto.CommandId, result.CommandId);
            Assert.AreEqual(expectedPlanningDto.DayExceptions, result.DayExceptions);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPlanningShouldThrowResourceNotFoundException()
        {
            var expectedPlanning = Fixture.Create<Planning>();
            var expectedPlanningDto = Mapper.Map<PlanningDto>(expectedPlanning);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(expectedPlanningDto.Id))
                .ReturnsAsync((Planning)null);

            var act = () => this.planningService.GetPlanning(expectedPlanningDto.Id);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The planning with id {expectedPlanning.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPlanningsShouldReturnPlanningList()
        {
            var expectedTotalPlanningsCount = 50;
            var expectedDevices = Fixture.CreateMany<Planning>(expectedTotalPlanningsCount).ToList();

            await DbContext.AddRangeAsync(expectedDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockPlanningRepository.Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDevices);

            var result = await this.planningService.GetPlannings();

            _ = result.Count().Should().Be(expectedTotalPlanningsCount);
            MockRepository.VerifyAll();
        }
    }
}

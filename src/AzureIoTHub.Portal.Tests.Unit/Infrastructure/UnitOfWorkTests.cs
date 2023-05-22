// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure
{
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class UnitOfWorkTests : BackendUnitTest
    {
        private UnitOfWork<DbContext> unitOfWork;

        public override void Setup()
        {
            base.Setup();

            _ = ServiceCollection.AddSingleton<IDeviceModelRepository, DeviceModelRepository>();
            _ = ServiceCollection.AddSingleton<ILabelRepository, LabelRepository>();
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<IUnitOfWork, UnitOfWork<PortalDbContext>>();

            Services = ServiceCollection.BuildServiceProvider();

            this.unitOfWork = new UnitOfWork<DbContext>(DbContext);
        }

        [TestCase]
        public async Task SaveAsyncTest()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();
            var label = Fixture.Create<Label>();

            _ = await this.unitOfWork.Context.AddAsync(deviceModel);
            _ = await this.unitOfWork.Context.AddAsync(label);

            // Act
            await this.unitOfWork.SaveAsync();

            // Assert
            var savedDeviceModel = await this.unitOfWork.Context.Set<DeviceModel>().FindAsync(deviceModel.Id);
            _ = savedDeviceModel.Should().BeEquivalentTo(deviceModel);
            var savedLabel = await this.unitOfWork.Context.Set<Label>().FindAsync(label.Id);
            _ = savedLabel.Should().BeEquivalentTo(label);
        }
    }
}

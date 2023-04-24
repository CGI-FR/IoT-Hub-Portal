// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Services.AWS_Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.IoT;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities.AWS
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Services.AWS;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v1._0.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Azure.Devices;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ThingTypeServiceTest : BackendUnitTest
    {

        private Mock<IThingTypeRepository> mockThingTypeRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<AmazonIoTClient> client;

        private IThingTypeService thingTypeService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockThingTypeRepository = MockRepository.Create<IThingTypeRepository>();
            this.client = MockRepository.Create<AmazonIoTClient>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockThingTypeRepository.Object);
            _ = ServiceCollection.AddSingleton(this.client.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IThingTypeService, ThingTypeService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.thingTypeService = Services.GetRequiredService<IThingTypeService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateAThingTypeShouldReturnAValue()
        {
            // Arrange
            _ = new BulkRegistryOperationResult
            {
                IsSuccessful = true
            };
            var searchAttr1 = new ThingTypeSearchableAttDto()
            {
                Name = "test_1"
            };
            var searchAttr2 = new ThingTypeSearchableAttDto()
            {
                Name = "test_2"
            };

            var thingDevice = new ThingTypeDetails()
            {
                ThingTypeID = "thingtypeid",
                ThingTypeName = "thingTypeTest",
                ThingTypeDescription = "My new Thing type",
                Tags = new Dictionary<string, string>()
                 {
                     {"tag1", "tag_1" },
                     {"tag1", "tag_2" }
                 },
                ThingTypeSearchableAttDtos = new List<ThingTypeSearchableAttDto>(){searchAttr1, searchAttr2}
            };

        }
    }
}

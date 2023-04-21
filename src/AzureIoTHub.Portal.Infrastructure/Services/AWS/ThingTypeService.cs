// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Shared.Models.v1._0.AWS;

    public class ThingTypeService : IThingTypeService
    {
        private readonly IThingTypeRepository thingTypeRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly AmazonIoTClient client;


        public ThingTypeService(
            AmazonIoTClient client,
            IThingTypeRepository thingTypeRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork
        )
        {
            this.thingTypeRepository = thingTypeRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.client = client;
        }

        public async Task<ThingTypeDetails> CreateThingType(ThingTypeDetails thingType)
        {
            ArgumentNullException.ThrowIfNull(thingType, nameof(thingType));

            var searchableAttributes = new List<string>();

            foreach (var s in thingType.ThingTypeSearchableAttDtos)
            {
                searchableAttributes.Add(s.Name);
            }

            var createThingTypeRequest = new CreateThingTypeRequest
            {
                ThingTypeName = thingType.ThingTypeName,
                ThingTypeProperties = new ThingTypeProperties
                {
                    ThingTypeDescription = thingType.ThingTypeDescription,
                    SearchableAttributes = searchableAttributes
                }
            };
            var request = await this.client.CreateThingTypeAsync(createThingTypeRequest);
            if (request.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return await CreateThingTypeInDatabase(thingType);
            }
            else
            {
                throw new InternalServerErrorException("Thing Type is not created in Amazon IoT");
            }
        }
        private async Task<ThingTypeDetails> CreateThingTypeInDatabase(ThingTypeDetails thingType)
        {
            var thingTypeEntity = this.mapper.Map<ThingType>(thingType);

            await this.thingTypeRepository.InsertAsync(thingTypeEntity);
            await this.unitOfWork.SaveAsync();

            return thingType;
        }
    }
}

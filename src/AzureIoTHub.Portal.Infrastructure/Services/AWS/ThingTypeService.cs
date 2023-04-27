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
    using AzureIoTHub.Portal.Models.v10.AWS;

    public class ThingTypeService : IThingTypeService
    {
        private readonly IThingTypeRepository thingTypeRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAmazonIoT amazonIoTClient;


        public ThingTypeService(
            IAmazonIoT amazonIoTClient,
            IThingTypeRepository thingTypeRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork
        )
        {
            this.thingTypeRepository = thingTypeRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.amazonIoTClient = amazonIoTClient;
        }

        public async Task<ThingTypeDetails> CreateThingType(ThingTypeDetails thingType)
        {
            ArgumentNullException.ThrowIfNull(thingType, nameof(thingType));

            List<string> searchableAttributes = null!;
            List<Tag> tags = null!;

            if (thingType.ThingTypeSearchableAttDtos != null)
            {
                searchableAttributes = thingType.ThingTypeSearchableAttDtos.Select(s => s.Name).ToList();
            }
            if (thingType.Tags != null)
            {
                tags = thingType.Tags.Select(pair => new Tag
                {
                    Key = pair.Key,
                    Value = pair.Value
                }).ToList();
            }


            var createThingTypeRequest = new CreateThingTypeRequest
            {
                ThingTypeName = thingType.ThingTypeName,
                ThingTypeProperties = new ThingTypeProperties
                {
                    ThingTypeDescription = thingType.ThingTypeDescription,
                    SearchableAttributes = searchableAttributes
                },
                Tags = tags
            };
            var request = await this.amazonIoTClient.CreateThingTypeAsync(createThingTypeRequest);
            return request.HttpStatusCode == System.Net.HttpStatusCode.OK
                ? await CreateThingTypeInDatabase(thingType)
                : throw new InternalServerErrorException("Thing Type is not created in Amazon IoT");
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

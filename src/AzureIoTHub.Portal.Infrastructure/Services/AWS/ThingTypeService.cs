// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using Amazon.IoT;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Shared.Models.v1._0.AWS;

    public class ThingTypeService : ThingTypeServiceBase<ThingTypeDetails>
    {
        private readonly IThingTypeRepository thingTypeRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public ThingTypeService(
            AmazonIoTClient client,
            IThingTypeRepository thingTypeRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork
        ) : base(client)
        {
            this.thingTypeRepository = thingTypeRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        protected override async Task<ThingTypeDetails> CreateThingTypeInDatabase(ThingTypeDetails thingType)
        {
            var thingTypeEntity = this.mapper.Map<ThingType>(thingType);

            await this.thingTypeRepository.InsertAsync(thingTypeEntity);
            await this.unitOfWork.SaveAsync();

            return thingType;
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Shared.Models.v1._0.AWS;

    public abstract class ThingTypeServiceBase<TDto> : IThingTypeService<TDto>
        where TDto : IThingTypeDetails
    {
        private readonly AmazonIoTClient client;

        protected ThingTypeServiceBase(AmazonIoTClient client)
        {
            this.client = client;
        }
        public async Task<TDto> CreateThingType(TDto thingType)
        {
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

        protected abstract Task<TDto> CreateThingTypeInDatabase(TDto thingType);
    }
}

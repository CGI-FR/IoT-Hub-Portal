// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Jobs.AWS
{
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using Microsoft.Extensions.Logging;
    using Quartz;

    [DisallowConcurrentExecution]
    public class SyncThingTypesJob : IJob
    {
        private readonly IThingTypeRepository thingTypeRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly ILogger<SyncThingTypesJob> logger;

        public SyncThingTypesJob(
            IThingTypeRepository thingTypeRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IAmazonIoT amazonIoTClient,
            ILogger<SyncThingTypesJob> logger)
        {
            this.thingTypeRepository = thingTypeRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.amazonIoTClient = amazonIoTClient;
            this.logger = logger;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync Thing Types job");

                await SyncThingTypes();

                this.logger.LogInformation("End of sync Thing Types job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync Thing Types job has failed");
            }
        }

        private async Task SyncThingTypes()
        {
            var getAllAWSThingTypes = await GetAllAWSThingTypes();
            foreach (var thingTYpe in getAllAWSThingTypes)
            {
                await CreateOrUpdateThingType(thingTYpe);
            }

            foreach (var thingType in (await this.thingTypeRepository.GetAllAsync()).Where(thingType => !getAllAWSThingTypes.Exists(x => x.ThingTypeID == thingType.Id)))
            {
                this.thingTypeRepository.Delete(thingType.Id);
            }

            await this.unitOfWork.SaveAsync();
        }

        private async Task<List<ThingTypeDto>> GetAllAWSThingTypes()
        {
            var thingTypes = new List<ThingTypeDto>();

            var request = new ListThingTypesRequest();
            var response = await amazonIoTClient.ListThingTypesAsync(request);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException("The request of getting all thing types failed due to an error in the Amazon IoT API.");

            }
            else
            {
                foreach (var thingType in response.ThingTypes)
                {
                    var requestDescribeThingType = new DescribeThingTypeRequest
                    {
                        ThingTypeName = thingType.ThingTypeName,
                    };
                    var responseDescribeThingType  = await amazonIoTClient.DescribeThingTypeAsync(requestDescribeThingType);

                    if (responseDescribeThingType.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new InternalServerErrorException("The request of getting DescribeThingType failed due to an error in the Amazon IoT API.");

                    }
                    else
                    {
                        var getAllSearchableAttribute = GetAllSearchableAttributes(responseDescribeThingType);

                        //get All tags from ResourceArn
                        //Because we do not have possiblity to retreive the Tag from DescribeThingTypeResponse and ListThingTypesResponse too.
                        var tags = await GetAllThingTypeTags(responseDescribeThingType);

                        var getThingType = new ThingTypeDto
                        {
                            ThingTypeID = responseDescribeThingType.ThingTypeId,
                            ThingTypeName = responseDescribeThingType.ThingTypeName,
                            ThingTypeDescription = responseDescribeThingType.ThingTypeProperties.ThingTypeDescription,
                            ThingTypeSearchableAttDtos = getAllSearchableAttribute,
                            Deprecated = responseDescribeThingType.ThingTypeMetadata.Deprecated,
                            Tags = tags
                        };

                        thingTypes.Add(getThingType);
                    }
                }
            }

            return thingTypes;
        }

        //To Get All Searchable attributes for a Thing Type
        private static List<ThingTypeSearchableAttDto> GetAllSearchableAttributes(DescribeThingTypeResponse thingType)
        {
            var searchableAttrDtos = new List<ThingTypeSearchableAttDto>();

            foreach (var searchAttr in thingType.ThingTypeProperties.SearchableAttributes)
            {
                var newSearchableAttr = new ThingTypeSearchableAttDto
                {
                    Name = searchAttr
                };
                searchableAttrDtos.Add(newSearchableAttr);
            }

            return searchableAttrDtos;
        }

        //To Get All tags for a thing types
        private async Task<List<ThingTypeTagDto>> GetAllThingTypeTags(DescribeThingTypeResponse thingType)
        {
            var listTagRequets = new ListTagsForResourceRequest
            {
                ResourceArn = thingType.ThingTypeArn
            };

            var thingTypeTags = await this.amazonIoTClient.ListTagsForResourceAsync(listTagRequets);

            var tags = new List<ThingTypeTagDto>();

            foreach (var tag in thingTypeTags.Tags)
            {
                var newTag = new ThingTypeTagDto
                {
                    Key = tag.Key,
                    Value = tag.Value
                };
                tags.Add(newTag);
            }

            return tags;
        }

        private async Task CreateOrUpdateThingType(ThingTypeDto thingTypeDto)
        {
            var existingTHingType = await this.thingTypeRepository.GetByIdAsync(thingTypeDto.ThingTypeID);
            var thingType = this.mapper.Map<ThingType>(thingTypeDto);

            if (existingTHingType == null)
            {
                await this.thingTypeRepository.InsertAsync(thingType);
            }
            else
            {
                this.thingTypeRepository.Update(thingType);
            }
        }

    }
}

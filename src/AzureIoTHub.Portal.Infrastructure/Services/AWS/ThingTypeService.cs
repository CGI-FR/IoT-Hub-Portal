// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System;
    using System.Linq.Expressions;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Http;
    using ResourceNotFoundException = Domain.Exceptions.ResourceNotFoundException;
    using AzureIoTHub.Portal.Domain.Repositories.AWS;

    public class ThingTypeService : IThingTypeService
    {
        private readonly IThingTypeRepository thingTypeRepository;
        private readonly IThingTypeTagRepository thingTypeTagRepository;
        private readonly IThingTypeSearchableAttRepository thingTypeSearchableAttributeRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IDeviceModelImageManager thingTypeImageManager;


        public ThingTypeService(
            IAmazonIoT amazonIoTClient,
            IThingTypeRepository thingTypeRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceModelImageManager thingTypeImageManager,
            IThingTypeTagRepository thingTypeTagRepository,
            IThingTypeSearchableAttRepository thingTypeSearchableAttributeRepository

        )
        {
            this.thingTypeRepository = thingTypeRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.amazonIoTClient = amazonIoTClient;
            this.thingTypeImageManager = thingTypeImageManager;
            this.thingTypeTagRepository = thingTypeTagRepository;
            this.thingTypeSearchableAttributeRepository = thingTypeSearchableAttributeRepository;
        }

        public async Task<PaginatedResult<ThingTypeDto>> GetThingTypes(DeviceModelFilter deviceModelFilter)
        {
            var thingTypePredicate = PredicateBuilder.True<ThingType>();

            if (!string.IsNullOrWhiteSpace(deviceModelFilter.SearchText))
            {
                thingTypePredicate = thingTypePredicate.And(thingType => thingType.Name.ToLower().Contains(deviceModelFilter.SearchText.ToLower())
                || thingType.Description.ToLower().Contains(deviceModelFilter.SearchText.ToLower())
                || thingType.Tags.Any(
                    tag => tag.Key.ToLower().Contains(deviceModelFilter.SearchText.ToLower())
                    || tag.Value.ToLower().Contains(deviceModelFilter.SearchText.ToLower()))
                || thingType.ThingTypeSearchableAttributes.Any(
                    attr => attr.Name.ToLower().Contains(deviceModelFilter.SearchText.ToLower())

                ));
            }

            var paginatedThingType = await this.thingTypeRepository.GetPaginatedListAsync(deviceModelFilter.PageNumber, deviceModelFilter.PageSize, deviceModelFilter.OrderBy, thingTypePredicate, includes: new Expression<Func<ThingType, object>>[] { d => d.ThingTypeSearchableAttributes});

            var paginatedThingTypeDto = new PaginatedResult<ThingTypeDto>
            {
                Data = paginatedThingType?.Data?.Select(x => this.mapper.Map<ThingTypeDto>(x, opts =>
                {
                    opts.AfterMap((src, dest) => dest.ImageUrl = this.thingTypeImageManager.ComputeImageUri(x.Id));
                })).ToList(),
                TotalCount = paginatedThingType.TotalCount,
                CurrentPage = paginatedThingType.CurrentPage,
                PageSize = deviceModelFilter.PageSize
            };

            return new PaginatedResult<ThingTypeDto>(paginatedThingTypeDto.Data, paginatedThingTypeDto.TotalCount, paginatedThingTypeDto.CurrentPage, paginatedThingType.PageSize);
        }

        public async Task<ThingTypeDto> GetThingType(string thingTypeId)
        {
            var getThingType = await this.thingTypeRepository.GetByIdAsync(thingTypeId, d => d.Tags!, d => d.ThingTypeSearchableAttributes!);
            if (getThingType == null)
            {
                throw new ResourceNotFoundException($"The thing type with id {thingTypeId} doesn't exist");

            }
            var getAvatar = this.thingTypeImageManager.ComputeImageUri(thingTypeId);

            var dto = this.mapper.Map<ThingTypeDto>(getThingType);
            dto.ImageUrl = getAvatar;

            return dto;
        }
        public async Task<string> CreateThingType(ThingTypeDto thingType)
        {
            ArgumentNullException.ThrowIfNull(thingType, nameof(thingType));

            var createThingTypeRequest = this.mapper.Map<CreateThingTypeRequest>(thingType);

            var response = await this.amazonIoTClient.CreateThingTypeAsync(createThingTypeRequest);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException("The creation of the thing type failed due to an error in the Amazon IoT API.");
            }
            else
            {
                thingType.ThingTypeID = response.ThingTypeId;
                return await CreateThingTypeInDatabase(thingType);
            }
        }
        private async Task<string> CreateThingTypeInDatabase(ThingTypeDto thingType)
        {
            var thingTypeEntity = this.mapper.Map<ThingType>(thingType);

            var GetThingType = this.thingTypeRepository.InsertAndGetIdAsync(thingTypeEntity);
            await this.unitOfWork.SaveAsync();

            _ = await this.thingTypeImageManager.SetDefaultImageToModel(thingType.ThingTypeID);

            return await GetThingType;
        }

        public async Task<ThingTypeDto> DeprecateThingType(string thingTypeId)
        {
            var getThingType = await this.thingTypeRepository.GetByIdAsync(thingTypeId, d => d.Tags!, d => d.ThingTypeSearchableAttributes!);
            if (getThingType == null)
            {
                throw new ResourceNotFoundException($"The thing type with id {thingTypeId} doesn't exist");

            }
            var deprecated = new DeprecateThingTypeRequest()
            {
                ThingTypeName = getThingType.Name,
                UndoDeprecate = false
            };

            var response = await this.amazonIoTClient.DeprecateThingTypeAsync(deprecated);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException("The deprecation of the thing type failed due to an error in the Amazon IoT API.");
            }
            else
            {
                getThingType.Deprecated = true;
                this.thingTypeRepository.Update(getThingType);
                await this.unitOfWork.SaveAsync();

                return this.mapper.Map<ThingTypeDto>(getThingType);
            }
        }

        public async Task DeleteThingType(string thingTypeId)
        {
            var getThingType = await this.thingTypeRepository.GetByIdAsync(thingTypeId, d => d.Tags!, d => d.ThingTypeSearchableAttributes!);
            if (getThingType == null)
            {
                throw new ResourceNotFoundException($"The thing type with id {thingTypeId} doesn't exist");

            }
            var deleted = new DeleteThingTypeRequest()
            {
                ThingTypeName = getThingType.Name
            };

            var response = await this.amazonIoTClient.DeleteThingTypeAsync(deleted);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException("The deletion of the thing type failed due to an error in the Amazon IoT API.");
            }
            else
            {
                await DeleteThingTypeInDatabase(getThingType);
            }

        }

        private async Task DeleteThingTypeInDatabase(ThingType thingType)
        {
            if (thingType.Tags != null
                && thingType.Tags?.Count != 0)
            {
                foreach (var tag in thingType.Tags!)
                {
                    this.thingTypeTagRepository.Delete(tag.Id);

                }
            }
            if (thingType.ThingTypeSearchableAttributes != null
                && thingType.ThingTypeSearchableAttributes?.Count != 0)
            {
                foreach (var search in thingType.ThingTypeSearchableAttributes!)
                {
                    this.thingTypeSearchableAttributeRepository.Delete(search.Id);

                }
            }

            this.thingTypeRepository.Delete(thingType.Id);
            await this.unitOfWork.SaveAsync();
            _ = this.thingTypeImageManager.DeleteDeviceModelImageAsync(thingType.Id);
        }
        public Task<string> GetThingTypeAvatar(string thingTypeId)
        {
            return Task.Run(() => this.thingTypeImageManager.ComputeImageUri(thingTypeId).ToString());
        }

        public Task<string> UpdateThingTypeAvatar(string thingTypeId, IFormFile file)
        {
            return Task.Run(() => this.thingTypeImageManager.ChangeDeviceModelImageAsync(thingTypeId, file.OpenReadStream()));
        }

        public Task DeleteThingTypeAvatar(string thingTypeId)
        {
            return this.thingTypeImageManager.DeleteDeviceModelImageAsync(thingTypeId);
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using Domain;
    using Domain.Entities;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Shared.Models.v1._0;

    public class DeviceTagService : IDeviceTagService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceTagRepository deviceTagRepository;

        public DeviceTagService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceTagRepository deviceTagRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceTagRepository = deviceTagRepository;
        }

        public IEnumerable<DeviceTagDto> GetAllTags()
        {
            return this.deviceTagRepository
                .GetAll()
                .Select(tag => this.mapper.Map<DeviceTagDto>(tag))
                .ToList();
        }

        public IEnumerable<string> GetAllTagsNames()
        {
            return this.deviceTagRepository
                .GetAll()
                .Select(tag => tag.Name)
                .ToList();
        }

        public IEnumerable<string> GetAllSearchableTagsNames()
        {
            return this.deviceTagRepository
                .GetAll()
                .Where(tag => tag.Searchable)
                .Select(tag => tag.Name)
                .ToList();
        }

        public async Task UpdateTags(IEnumerable<DeviceTagDto> tags)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(tags);

                var existingTags = this.deviceTagRepository.GetAll().ToList();

                existingTags.ForEach(tag =>
                {
                    this.deviceTagRepository.Delete(tag.Id);
                });

                foreach (var tag in tags)
                {
                    await this.deviceTagRepository.InsertAsync(this.mapper.Map<DeviceTag>(tag));
                }

                await this.unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException("Unable to save devices tags", e);
            }
        }

        public async Task CreateOrUpdateDeviceTag(DeviceTagDto deviceTag)
        {
            try
            {
                var deviceTagEntity = await this.deviceTagRepository.GetByIdAsync(deviceTag.Name);

                if (deviceTagEntity == null)
                {
                    deviceTagEntity = this.mapper.Map<DeviceTag>(deviceTag);
                    await this.deviceTagRepository.InsertAsync(deviceTagEntity);
                }
                else
                {
                    deviceTagEntity.Label = deviceTag.Label;
                    deviceTagEntity.Searchable = deviceTag.Searchable;
                    deviceTagEntity.Required = deviceTag.Required;

                    this.deviceTagRepository.Update(deviceTagEntity);
                }

                await this.unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to create or update the device tag {deviceTag.Name}", e);
            }
        }

        public async Task DeleteDeviceTagByName(string deviceTagName)
        {
            try
            {
                this.deviceTagRepository.Delete(deviceTagName);

                await this.unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to delete the device tag {deviceTagName}", e);
            }
        }
    }
}

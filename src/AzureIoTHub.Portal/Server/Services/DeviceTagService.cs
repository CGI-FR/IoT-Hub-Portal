// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Mappers;

    public class DeviceTagService : IDeviceTagService
    {
        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device tag mapper.
        /// </summary>
        private readonly IDeviceTagMapper deviceTagMapper;

        /// <summary>
        /// The default partition key in AzureDataTable
        /// </summary>
        public const string DefaultPartitionKey = "0";

        public DeviceTagService(IDeviceTagMapper deviceTagMapper, ITableClientFactory tableClientFactory)
        {
            this.deviceTagMapper = deviceTagMapper;
            this.tableClientFactory = tableClientFactory;
        }

        public IEnumerable<DeviceTag> GetAllTags()
        {
            try
            {
                return this.tableClientFactory
                    .GetDeviceTagSettings()
                    .Query<TableEntity>()
                    .Select(this.deviceTagMapper.GetDeviceTag)
                    .ToList();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get devices tags", e);
            }
        }

        public IEnumerable<string> GetAllTagsNames()
        {
            try
            {
                var tagNameList = this.tableClientFactory
                    .GetDeviceTagSettings()
                    .Query<TableEntity>()
                    .Select(c => this.deviceTagMapper.GetDeviceTag(c).Name);

                return tagNameList.ToList();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to query device tags names: {e.Message}", e);
            }
        }

        public IEnumerable<string> GetAllSearchableTagsNames()
        {
            try
            {
                var tagNameList = this.tableClientFactory
                    .GetDeviceTagSettings()
                    .Query<TableEntity>()
                    .Where(c => this.deviceTagMapper.GetDeviceTag(c).Searchable)
                    .Select(c => this.deviceTagMapper.GetDeviceTag(c).Name);

                return tagNameList.ToList();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to query searchable device tags names: {e.Message}", e);
            }
        }

        public async Task UpdateTags(IEnumerable<DeviceTag> tags)
        {
            ArgumentNullException.ThrowIfNull(tags, nameof(tags));

            Pageable<TableEntity> query;

            try
            {
                query = this.tableClientFactory
                    .GetDeviceTagSettings()
                    .Query<TableEntity>();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get existing devices tags", e);
            }

            foreach (var item in query)
            {
                try
                {
                    _ = await this.tableClientFactory
                        .GetDeviceTagSettings()
                        .DeleteEntityAsync(item.PartitionKey, item.RowKey);
                }
                catch (RequestFailedException e)
                {
                    throw new InternalServerErrorException($"Unable to delete the device tag {item.RowKey}", e);
                }
            }

            foreach (var tag in tags)
            {
                var entity = new TableEntity()
                {
                    PartitionKey = DefaultPartitionKey,
                    RowKey = tag.Name
                };
                await SaveEntity(entity, tag);
            }
        }

        public async Task CreateOrUpdateDeviceTag(DeviceTag deviceTag)
        {
            var entity = new TableEntity
            {
                PartitionKey = DefaultPartitionKey,
                RowKey = deviceTag.Name
            };

            this.deviceTagMapper.UpdateTableEntity(entity, deviceTag);

            try
            {
                _ = await this.tableClientFactory
                    .GetDeviceTagSettings()
                    .UpsertEntityAsync(entity);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to create or update the device tag {deviceTag.Name}", e);
            }
        }

        public async Task DeleteDeviceTagByName(string deviceTagName)
        {

            try
            {
                _ = await this.tableClientFactory
                    .GetDeviceTagSettings()
                    .DeleteEntityAsync(DefaultPartitionKey, deviceTagName);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to delete the device tag {deviceTagName}", e);
            }
        }

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="tag">The device tag</param>
        private async Task SaveEntity(TableEntity entity, DeviceTag tag)
        {
            this.deviceTagMapper.UpdateTableEntity(entity, tag);

            try
            {
                _ = await this.tableClientFactory
                    .GetDeviceTagSettings()
                    .AddEntityAsync(entity);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to save the device tag {tag.Name}", e);
            }
        }
    }
}

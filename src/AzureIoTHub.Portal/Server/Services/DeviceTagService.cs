// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.v10.Device;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
            var tagList = this.tableClientFactory
                            .GetDeviceTagSettings()
                            .Query<TableEntity>()
                            .Select(this.deviceTagMapper.GetDeviceTag);

            return tagList.ToList();
        }

        public IEnumerable<string> GetAllTagsNames()
        {
            var tagNameList = this.tableClientFactory
                .GetDeviceTagSettings()
                .Query<TableEntity>()
                .Select(c => this.deviceTagMapper.GetDeviceTag(c).Name);

            return tagNameList.ToList();
        }

        public IEnumerable<string> GetAllSearchableTagsNames()
        {
            var tagNameList = this.tableClientFactory
                .GetDeviceTagSettings()
                .Query<TableEntity>()
                .Where(c => this.deviceTagMapper.GetDeviceTag(c).Searchable)
                .Select(c => this.deviceTagMapper.GetDeviceTag(c).Name);

            return tagNameList.ToList();
        }

        public async Task UpdateTags(List<DeviceTag> tags)
        {
            var query = this.tableClientFactory
                        .GetDeviceTagSettings()
                        .Query<TableEntity>();

            foreach (var item in query)
            {
                _ = await this.tableClientFactory
                    .GetDeviceTagSettings()
                    .DeleteEntityAsync(item.PartitionKey, item.RowKey);
            }

            foreach (var tag in tags)
            {
                var entity = new TableEntity()
                {
                    PartitionKey = DefaultPartitionKey,
                    RowKey = tag.Name
                };
                await this.SaveEntity(entity, tag);
            }
        }

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="tag">The device tag</param>
        /// <returns></returns>
        private async Task SaveEntity(TableEntity entity, DeviceTag tag)
        {
            this.deviceTagMapper.UpdateTableEntity(entity, tag);
            _ = await this.tableClientFactory
                .GetDeviceTagSettings()
                .AddEntityAsync(entity);
        }
    }
}

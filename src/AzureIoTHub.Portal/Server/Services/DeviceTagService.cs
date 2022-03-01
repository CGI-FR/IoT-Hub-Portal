using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Services
{
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

        public async Task UpdateTags(List<DeviceTag> tags)
        {
            var query = this.tableClientFactory
                        .GetDeviceTagSettings()
                        .Query<TableEntity>()
                        .AsPages();

            foreach (var page in query)
            {
                foreach (var item in page.Values)
                {
                    await this.tableClientFactory
                        .GetDeviceTagSettings()
                        .DeleteEntityAsync(item.PartitionKey, item.RowKey);
                }
            }

            foreach (DeviceTag tag in tags)
            {
                TableEntity entity = new TableEntity()
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
            await this.tableClientFactory
                .GetDeviceTagSettings()
                .AddEntityAsync(entity);
        }
    }
}

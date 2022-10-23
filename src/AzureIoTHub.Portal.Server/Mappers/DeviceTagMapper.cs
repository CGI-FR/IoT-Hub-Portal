// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;

    public class DeviceTagMapper : IDeviceTagMapper
    {
        /// <summary>
        /// Gets a device tag setting.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A device tag setting</returns>
        public DeviceTagDto GetDeviceTag(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new DeviceTagDto
            {
                Name = entity.RowKey,
                Label = entity[nameof(DeviceTagDto.Label)].ToString(),
                Required = bool.Parse(entity[nameof(DeviceTagDto.Required)].ToString() ?? "false"),
                Searchable = bool.Parse(entity[nameof(DeviceTagDto.Searchable)].ToString() ?? "false")
            };
        }

        /// <summary>
        /// Updates the table entity.
        /// </summary>
        /// <param name="tagEntity">The entity.</param>
        /// <param name="element">The device tag object.</param>
        public void UpdateTableEntity(TableEntity tagEntity, DeviceTagDto element)
        {
            ArgumentNullException.ThrowIfNull(tagEntity, nameof(tagEntity));
            ArgumentNullException.ThrowIfNull(element, nameof(element));

            tagEntity[nameof(DeviceTagDto.Label)] = element.Label;
            tagEntity[nameof(DeviceTagDto.Required)] = element.Required;
            tagEntity[nameof(DeviceTagDto.Searchable)] = element.Searchable;
        }
    }
}

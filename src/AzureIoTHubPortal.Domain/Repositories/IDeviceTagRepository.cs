// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Repositories
{
    using Entities;

    public interface IDeviceTagRepository : IRepository<DeviceTag>
    {
        Task<bool> DeviceTagExists(string deviceTagId);

        Task<DeviceTag> CreateDeviceTag(DeviceTag deviceTag);

        Task<DeviceTag> UpdateDeviceTag(DeviceTag deviceTag);

        Task<DeviceTag> CreateOrUpdateTag(DeviceTag deviceTag);
    }
}

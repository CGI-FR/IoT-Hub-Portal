// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using AzureIoTHub.Portal.Models.v10;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDeviceTagService
    {
        IEnumerable<DeviceTag> GetAllTags();

        IEnumerable<string> GetAllTagsNames();

        IEnumerable<string> GetAllSearchableTagsNames();

        Task UpdateTags(IEnumerable<DeviceTag> tags);

        Task CreateOrUpdateDeviceTag(DeviceTag deviceTag);

        Task DeleteDeviceTagByName(string deviceTagName);
    }
}

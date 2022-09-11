// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Abstractions.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.v10;

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

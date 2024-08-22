// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shared.Models.v1._0;

    public interface IDeviceTagService
    {
        IEnumerable<DeviceTagDto> GetAllTags();

        IEnumerable<string> GetAllTagsNames();

        IEnumerable<string> GetAllSearchableTagsNames();

        Task UpdateTags(IEnumerable<DeviceTagDto> tags);

        Task CreateOrUpdateDeviceTag(DeviceTagDto deviceTag);

        Task DeleteDeviceTagByName(string deviceTagName);
    }
}

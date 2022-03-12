// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using AzureIoTHub.Portal.Shared.Models.v10.Device;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDeviceTagService
    {
        IEnumerable<DeviceTag> GetAllTags();

        IEnumerable<string> GetAllTagsNames();

        IEnumerable<string> GetAllSearchableTagsNames();

        Task UpdateTags(List<DeviceTag> tags);
    }
}

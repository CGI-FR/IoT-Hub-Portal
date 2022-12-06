// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services
{
    using AzureIoTHub.Portal.Domain.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDeviceModelPropertiesService
    {
        Task<IEnumerable<DeviceModelProperty>> GetModelProperties(string modelId);
        Task SavePropertiesForModel(string modelId, IEnumerable<DeviceModelProperty> items);

        IEnumerable<string> GetAllPropertiesNames();
    }
}

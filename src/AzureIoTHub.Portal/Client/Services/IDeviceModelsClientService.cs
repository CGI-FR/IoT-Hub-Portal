// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Portal.Models.v10;

    public interface IDeviceModelsClientService
    {
        Task<IList<DeviceModelDto>> GetDeviceModels();

        Task<DeviceModelDto> GetDeviceModel(string deviceModelId);

        Task CreateDeviceModel(DeviceModelDto deviceModel);

        Task UpdateDeviceModel(DeviceModelDto deviceModel);

        Task DeleteDeviceModel(string deviceModelId);

        Task<IList<DeviceProperty>> GetDeviceModelModelProperties(string deviceModelId);

        Task SetDeviceModelModelProperties(string deviceModelId, IList<DeviceProperty> deviceProperties);

        Task<string> GetAvatarUrl(string deviceModelId);

        Task ChangeAvatar(string deviceModelId, MultipartFormDataContent avatar);
    }
}

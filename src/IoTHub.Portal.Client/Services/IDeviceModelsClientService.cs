// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public interface IDeviceModelsClientService
    {
        Task<PaginationResult<DeviceModelDto>> GetDeviceModelsAsync(DeviceModelFilter? deviceModelFilter = null);

        Task<DeviceModelDto> GetDeviceModel(string deviceModelId);

        Task<DeviceModelDto> CreateDeviceModelAsync(DeviceModelDto deviceModel);

        Task UpdateDeviceModel(DeviceModelDto deviceModel);

        Task DeleteDeviceModel(string deviceModelId);

        Task<IList<DeviceProperty>> GetDeviceModelModelPropertiesAsync(string deviceModelId);

        Task SetDeviceModelModelProperties(string deviceModelId, IList<DeviceProperty> deviceProperties);

        Task<string> GetAvatar(string deviceModelId);

        Task ChangeAvatarAsync(string deviceModelId, StringContent avatar);
    }
}

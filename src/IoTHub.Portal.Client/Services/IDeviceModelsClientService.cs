// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public interface IDeviceModelsClientService
    {
        Task<PaginationResult<DeviceModelDto>> GetDeviceModels(DeviceModelFilter? deviceModelFilter = null);

        Task<DeviceModelDto> GetDeviceModel(string deviceModelId);

        Task<DeviceModelDto> CreateDeviceModel(DeviceModelDto deviceModel);

        Task UpdateDeviceModel(DeviceModelDto deviceModel);

        Task DeleteDeviceModel(string deviceModelId);

        Task<IList<DeviceProperty>> GetDeviceModelModelProperties(string deviceModelId);

        Task SetDeviceModelModelProperties(string deviceModelId, IList<DeviceProperty> deviceProperties);

        Task<string> GetAvatarUrl(string deviceModelId);

        Task ChangeAvatar(string deviceModelId, MultipartFormDataContent avatar);
    }
}

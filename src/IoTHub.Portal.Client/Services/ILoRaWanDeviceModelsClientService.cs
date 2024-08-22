// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Portal.Shared.Models.v1._0.LoRaWAN;

    public interface ILoRaWanDeviceModelsClientService
    {
        Task<LoRaDeviceModelDto> GetDeviceModel(string deviceModelId);

        Task CreateDeviceModel(LoRaDeviceModelDto deviceModelDto);

        Task UpdateDeviceModel(LoRaDeviceModelDto deviceModelDto);

        Task SetDeviceModelCommands(string deviceModelId, IList<DeviceModelCommandDto> commands);

        Task<IList<DeviceModelCommandDto>> GetDeviceModelCommands(string deviceModelId);

        Task<string> GetAvatarUrl(string deviceModelId);

        Task ChangeAvatar(string deviceModelId, MultipartFormDataContent avatar);
    }
}

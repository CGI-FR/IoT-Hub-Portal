// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Portal.Models.v10.LoRaWAN;

    public interface ILoRaWanDeviceModelsClientService
    {
        Task<LoRaDeviceModel> GetDeviceModel(string deviceModelId);

        Task CreateDeviceModel(LoRaDeviceModel deviceModel);

        Task UpdateDeviceModel(LoRaDeviceModel deviceModel);

        Task SetDeviceModelCommands(string deviceModelId, IList<DeviceModelCommandDto> commands);

        Task<IList<DeviceModelCommandDto>> GetDeviceModelCommands(string deviceModelId);

        Task<string> GetAvatarUrl(string deviceModelId);

        Task ChangeAvatar(string deviceModelId, MultipartFormDataContent avatar);
    }
}

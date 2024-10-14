// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public interface IDeviceTagSettingsClientService
    {
        Task<IList<DeviceTagDto>> GetDeviceTags();

        Task CreateOrUpdateDeviceTag(DeviceTagDto deviceTag);

        Task DeleteDeviceTagByName(string deviceTagName);
    }
}

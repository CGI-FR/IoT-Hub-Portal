// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Managers
{
    public interface IDeviceModelImageManager
    {
        Task<string> GetDeviceModelImageAsync(string deviceModelId);

        Task<string> ChangeDeviceModelImageAsync(string deviceModelId, string file);

        Task DeleteDeviceModelImageAsync(string deviceModelId);

        Task InitializeDefaultImageBlob();

        Task SyncImagesCacheControl();

        Task<string> SetDefaultImageToModel(string deviceModelId);
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Managers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IDeviceModelImageManager
    {
        Task<string> ChangeDeviceModelImageAsync(string deviceModelId, Stream stream);

        Task DeleteDeviceModelImageAsync(string deviceModelId);

        Uri ComputeImageUri(string deviceModelId);

        Task InitializeDefaultImageBlob();

        Task SyncImagesCacheControl();

        Task<string> SetDefaultImageToModel(string deviceModelId);
    }
}

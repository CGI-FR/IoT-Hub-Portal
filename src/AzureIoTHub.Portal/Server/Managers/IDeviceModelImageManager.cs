// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IDeviceModelImageManager
    {
        Uri ComputeImageUri(string deviceModelId);

        Task<Uri> ChangeDeviceModelImageAsync(string deviceModelId, Stream stream);

        Task InitializeDefaultImageBlob();
    }
}

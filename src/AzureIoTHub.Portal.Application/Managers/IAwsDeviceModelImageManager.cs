// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Managers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public interface IAwsDeviceModelImageManager
    {
        Task<string> ChangeDeviceModelImageAsync(string deviceModelId, IFormFile file, string bucketName);

        Task DeleteDeviceModelImageAsync(string deviceModelId, string bucketName);

        Uri ComputeImageUri(string deviceModelId, string bucketName);

        Task InitializeDefaultImageBlob(string bucketName);

        Task SyncImagesCacheControl(string bucketName);

        Task<string> SetDefaultImageToModel(string deviceModelId, string bucketName);
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Managers
{
    using System.IO;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using IoTHub.Portal.Application.Managers;
    using Domain;
    using Domain.Exceptions;
    using Domain.Options;
    using Microsoft.Extensions.Logging;
    using Shared.Constants;

    public class DeviceModelImageManager : IDeviceModelImageManager
    {
        private readonly BlobServiceClient blobService;
        private readonly ILogger<DeviceModelImageManager> logger;
        private readonly ConfigHandler configHandler;

        public DeviceModelImageManager(
            ILogger<DeviceModelImageManager> logger,
            BlobServiceClient blobService,
            ConfigHandler configHandler)
        {
            this.logger = logger;
            this.blobService = blobService;
            this.configHandler = configHandler;
        }

        public async Task<string> GetDeviceModelImageAsync(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(DeviceModelImageOptions.ImageContainerName);
            var blobClient = blobContainer.GetBlobClient(deviceModelId);
            try
            {
                using var reader = new StreamReader((await blobClient.DownloadAsync()).Value.Content);
                return await reader.ReadToEndAsync();
            }
            catch (RequestFailedException e) when (e.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                this.logger.LogError(e, $"Blob for device model '{deviceModelId}' not found.");
                return DeviceModelImageOptions.DefaultImage;
            }
        }

        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, string file)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(DeviceModelImageOptions.ImageContainerName);
            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Name}\n");

            _ = await blobClient.UploadAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(file)), true);
            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            return file;
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(DeviceModelImageOptions.ImageContainerName);
            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Name}\n");

            _ = await blobClient.UploadAsync(DeviceModelImageOptions.DefaultImageStream, true);

            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            return DeviceModelImageOptions.DefaultImage;
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(DeviceModelImageOptions.ImageContainerName);
            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Deleting from Blob storage :\n\t {blobClient.Name}\n");

            try
            {
                _ = await blobClient.DeleteIfExistsAsync();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to delete the image from the blob storage.", e);
            }
        }

        public async Task InitializeDefaultImageBlob()
        {
            var container = this.blobService.GetBlobContainerClient(DeviceModelImageOptions.ImageContainerName);
            var blobClient = container.GetBlobClient(DeviceModelImageOptions.DefaultImageName);

            _ = await blobClient.UploadAsync(DeviceModelImageOptions.DefaultImageStream, overwrite: true);
        }

        public async Task SyncImagesCacheControl()
        {
            var container = this.blobService.GetBlobContainerClient(DeviceModelImageOptions.ImageContainerName);

            await foreach (var blob in container.GetBlobsAsync())
            {
                var blobClient = container.GetBlobClient(blob.Name);

                _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });
            }
        }

    }
}

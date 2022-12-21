// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Managers
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Options;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class DeviceModelImageManager : IDeviceModelImageManager
    {
        private readonly BlobServiceClient blobService;
        private readonly ILogger<DeviceModelImageManager> logger;
        private readonly ConfigHandler configHandler;

        private readonly IOptions<DeviceModelImageOptions> deviceModelImageOptions;

        public DeviceModelImageManager(
            ILogger<DeviceModelImageManager> logger,
            BlobServiceClient blobService,
            ConfigHandler configHandler,
            IOptions<DeviceModelImageOptions> BaseImageOption)
        {
            this.logger = logger;
            this.blobService = blobService;
            this.configHandler = configHandler;

            this.deviceModelImageOptions = BaseImageOption;
        }

        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, Stream stream)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            _ = await blobClient.UploadAsync(stream, true);

            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            return blobClient.Uri.ToString();
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            var currentAssembly = Assembly.GetExecutingAssembly();

            var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{this.deviceModelImageOptions.Value.DefaultImageName}");

            _ = await blobClient.UploadAsync(defaultImageStream, true);

            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Deleting from Blob storage :\n\t {blobClient.Uri}\n");

            try
            {
                _ = await blobClient.DeleteIfExistsAsync();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to delete the image from the blob storage.", e);
            }
        }

        public Uri ComputeImageUri(string deviceModelId)
        {
            return new Uri(this.deviceModelImageOptions.Value.BaseUri, $"{this.deviceModelImageOptions.Value.BaseUri}/{deviceModelId}");
        }

        public async Task InitializeDefaultImageBlob()
        {
            var container = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);

            var blobClient = container.GetBlobClient(this.deviceModelImageOptions.Value.DefaultImageName);

            var currentAssembly = Assembly.GetExecutingAssembly();

            using var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{this.deviceModelImageOptions.Value.DefaultImageName}");

            _ = await blobClient.UploadAsync(defaultImageStream, overwrite: true);
        }

        public async Task SyncImagesCacheControl()
        {
            var container = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);

            await foreach (var blob in container.GetBlobsAsync())
            {
                var blobClient = container.GetBlobClient(blob.Name);

                _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });
            }
        }
    }
}

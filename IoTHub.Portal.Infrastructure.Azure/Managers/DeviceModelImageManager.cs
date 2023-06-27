// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Managers
{
    using global::Azure;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Options;
    using IoTHub.Portal.Infrastructure.Common;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

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

            deviceModelImageOptions = BaseImageOption;
        }

        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, Stream stream)
        {
            var blobContainer = blobService.GetBlobContainerClient(deviceModelImageOptions.Value.ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            _ = await blobClient.UploadAsync(stream, true);

            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            return blobClient.Uri.ToString();
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {
            var blobContainer = blobService.GetBlobContainerClient(deviceModelImageOptions.Value.ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            var currentAssembly = Assembly.GetExecutingAssembly();

            var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{deviceModelImageOptions.Value.DefaultImageName}");

            _ = await blobClient.UploadAsync(defaultImageStream, true);

            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {
            var blobContainer = blobService.GetBlobContainerClient(deviceModelImageOptions.Value.ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            logger.LogInformation($"Deleting from Blob storage :\n\t {blobClient.Uri}\n");

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
            return new Uri(deviceModelImageOptions.Value.BaseUri, $"{deviceModelImageOptions.Value.BaseUri}/{deviceModelId}");
        }

        public async Task InitializeDefaultImageBlob()
        {
            var container = blobService.GetBlobContainerClient(deviceModelImageOptions.Value.ImageContainerName);

            var blobClient = container.GetBlobClient(deviceModelImageOptions.Value.DefaultImageName);

            var currentAssembly = Assembly.GetAssembly(typeof(ConfigHandlerBase));

            using var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{deviceModelImageOptions.Value.DefaultImageName}");

            _ = await blobClient.UploadAsync(defaultImageStream, overwrite: true);
        }

        public async Task SyncImagesCacheControl()
        {
            var container = blobService.GetBlobContainerClient(deviceModelImageOptions.Value.ImageContainerName);

            await foreach (var blob in container.GetBlobsAsync())
            {
                var blobClient = container.GetBlobClient(blob.Name);

                _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });
            }
        }

    }
}

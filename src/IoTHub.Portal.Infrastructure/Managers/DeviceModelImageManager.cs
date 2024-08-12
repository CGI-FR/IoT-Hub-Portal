// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Managers
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Options;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using StackExchange.Redis;

    public class DeviceModelImageManager : IDeviceModelImageManager
    {
        private readonly BlobServiceClient blobService;
        private readonly ILogger<DeviceModelImageManager> logger;
        private readonly ConfigHandler configHandler;
        //private readonly IDatabase redisDb;
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

            // Todo Add Redis connection string to configuration
            //var redisConnection = ConnectionMultiplexer.Connect("");
            //redisDb = redisConnection.GetDatabase();

            this.deviceModelImageOptions = BaseImageOption;
        }

        public async Task<string> GetDeviceModelImageAsync(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);
            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            using var reader = new StreamReader((await blobClient.DownloadAsync()).Value.Content);

            return await reader.ReadToEndAsync();
        }

        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, string file)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);
            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            _ = await blobClient.UploadAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(file)), true);
            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            return file;
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);
            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            //var defaultFilePath =
            //    $"{currentAssembly.GetName().Name}.Resources.{this.deviceModelImageOptions.Value.DefaultImageName}";

            //var defaultFile = File.Open(defaultFilePath, FileMode.Open);

            _ = await blobClient.UploadAsync(DeviceModelImageOptions.DefaultImageStream, true);

            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            //var imageBase64 = ComputeImageBase64(new FormFile(defaultfile, 0 , defaultfile.Length, string.Empty, defaultfile.Name));

            //_ = redisDb.StringSet(deviceModelId, imageBase64);

            return DeviceModelImageOptions.DefaultImage;
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);
            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Deleting from Blob storage :\n\t {blobClient.Uri}\n");

            try
            {
                _ = await blobClient.DeleteIfExistsAsync();
                //_ = redisDb.KeyDeleteAsync(deviceModelId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to delete the image from the blob storage.", e);
            }
        }

        public async Task InitializeDefaultImageBlob()
        {
            var container = this.blobService.GetBlobContainerClient(this.deviceModelImageOptions.Value.ImageContainerName);
            var blobClient = container.GetBlobClient(this.deviceModelImageOptions.Value.DefaultImageName);

            _ = await blobClient.UploadAsync(DeviceModelImageOptions.DefaultImageStream, overwrite: true);

            //using var file = File.Open($"{currentAssembly.GetName().Name}.Resources.{this.deviceModelImageOptions.Value.DefaultImageName}", FileMode.Open);

            //_ = redisDb.StringSet(file.Name, ComputeImageBase64(new FormFile(file, 0, file.Length, string.Empty, file.Name)));
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

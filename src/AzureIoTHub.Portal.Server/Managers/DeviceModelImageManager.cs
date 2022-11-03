// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Microsoft.Extensions.Logging;

    public class DeviceModelImageManager : IDeviceModelImageManager
    {
        private const string ImageContainerName = "device-images";
        private const string DefaultImageName = "default-template-icon.png";

        private readonly BlobServiceClient blobService;
        private readonly ILogger<DeviceModelImageManager> logger;
        private readonly ConfigHandler configHandler;

        private readonly EnvVariableRegistry enVariableRegistry;

        public DeviceModelImageManager(
            ILogger<DeviceModelImageManager> logger,
            BlobServiceClient blobService,
            EnvVariableRegistry variableRegistry,
            ConfigHandler configHandler)
        {
            this.logger = logger;
            this.blobService = blobService;
            this.configHandler = configHandler;

            this.enVariableRegistry = variableRegistry;

            var blobClient = this.blobService.GetBlobContainerClient(ImageContainerName);

            _ = blobClient.SetAccessPolicy(PublicAccessType.Blob);
            _ = blobClient.CreateIfNotExists();
        }

        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, Stream stream)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            _ = await blobClient.UploadAsync(stream, true);

            _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(ImageContainerName);

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
            var imageName = string.IsNullOrWhiteSpace(deviceModelId) ? DefaultImageName : deviceModelId;

            return new Uri(this.enVariableRegistry.BaseImageFolderUri, $"{this.enVariableRegistry.BaseImageFolderUri}/{imageName}");

            // Checking if the image exists in the blob container
            //using (var request = new HttpRequestMessage(HttpMethod.Head, $"{this.variableRegistry.BaseImageFolderUri}/{imageName}"))
            //{
            //    using var client = new HttpClient();
            //    var response = client.Send(request);

            //    if (!response.IsSuccessStatusCode)
            //    {
            //        blobClient = container.GetBlobClient(DefaultImageName);
            //          return new Uri(this.variableRegistry.BaseImageFolderUri, $"{this.variableRegistry.BaseImageFolderUri}/{DefaultImageName}");
            //    }
            //}

            //return new Uri(this.variableRegistry.BaseImageFolderUri, $"{this.variableRegistry.BaseImageFolderUri}/{imageName}");
        }

        public async Task InitializeDefaultImageBlob()
        {
            var container = this.blobService.GetBlobContainerClient(ImageContainerName);

            var blobClient = container.GetBlobClient(DefaultImageName);

            var currentAssembly = Assembly.GetExecutingAssembly();

            using var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{DefaultImageName}");

            _ = await blobClient.UploadAsync(defaultImageStream, overwrite: true);
        }

        public async Task SyncImagesCacheControl()
        {
            var container = this.blobService.GetBlobContainerClient(ImageContainerName);

            await foreach (var blob in container.GetBlobsAsync())
            {
                var blobClient = container.GetBlobClient(blob.Name);

                _ = await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" });
            }
        }
    }
}

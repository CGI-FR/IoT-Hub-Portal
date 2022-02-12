// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Microsoft.Extensions.Logging;

    public class DeviceModelImageManager : IDeviceModelImageManager
    {
        private const string ImageContainerName = "device-images";
        private const string DefaultImageName = "default-template-icon.png";

        private readonly BlobServiceClient blobService;
        private readonly ILogger<DeviceModelImageManager> logger;

        public DeviceModelImageManager(ILogger<DeviceModelImageManager> logger, BlobServiceClient blobService)
        {
            this.logger = logger;
            this.blobService = blobService;

            var blobClient = this.blobService.GetBlobContainerClient(ImageContainerName);

            blobClient.SetAccessPolicy(PublicAccessType.Blob);
            blobClient.CreateIfNotExists();
        }

        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, Stream stream)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            _ = await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(deviceModelId);

            this.logger.LogInformation($"Deleting from Blob storage :\n\t {blobClient.Uri}\n");

            await blobClient.DeleteIfExistsAsync();
        }

        public string ComputeImageUri(string deviceModelId)
        {
            var imageName = string.IsNullOrWhiteSpace(deviceModelId) ? DefaultImageName : deviceModelId;

            var container = this.blobService.GetBlobContainerClient(ImageContainerName);
            var blobClient = container.GetBlobClient(imageName);

            // Checking if the image exists in the blob container
            using (var request = new HttpRequestMessage(HttpMethod.Head, blobClient.Uri.ToString()))
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = client.Send(request);

                if (!response.IsSuccessStatusCode)
                {
                    blobClient = container.GetBlobClient(DefaultImageName);
                }
            }

            return blobClient.Uri.ToString();
        }

        public async Task InitializeDefaultImageBlob()
        {
            var container = this.blobService.GetBlobContainerClient(ImageContainerName);

            var blobClient = container.GetBlobClient(DefaultImageName);

            var currentAssembly = Assembly.GetExecutingAssembly();

            using var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{DefaultImageName}");

            await blobClient.UploadAsync(defaultImageStream, overwrite: true);
        }
    }
}

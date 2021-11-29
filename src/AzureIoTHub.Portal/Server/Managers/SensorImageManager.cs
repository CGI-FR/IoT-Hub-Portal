// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Microsoft.Extensions.Logging;

    public class SensorImageManager : ISensorImageManager
    {
        private const string ImageContainerName = "device-images";
        private const string DefaultImageName = "default-template-icon.png";

        private readonly BlobServiceClient blobService;
        private readonly ILogger<SensorImageManager> logger;

        public SensorImageManager(ILogger<SensorImageManager> logger, BlobServiceClient blobService)
        {
            this.logger = logger;
            this.blobService = blobService;

            var blobClient = this.blobService.GetBlobContainerClient(ImageContainerName);

            blobClient.SetAccessPolicy(PublicAccessType.Blob);
            blobClient.CreateIfNotExists();
        }

        public async Task<Uri> ChangeSensorImageAsync(string sensorModelName, Stream stream)
        {
            var blobContainer = this.blobService.GetBlobContainerClient(ImageContainerName);

            var blobClient = blobContainer.GetBlobClient(sensorModelName);

            this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

            _ = await blobClient.UploadAsync(stream);

            return blobClient.Uri;
        }

        public Uri ComputeImageUri(string sensorModelName)
        {
            var imageName = string.IsNullOrWhiteSpace(sensorModelName) ? DefaultImageName : sensorModelName;

            var container = this.blobService.GetBlobContainerClient(ImageContainerName);
            var blobClient = container.GetBlobClient(imageName);

            return blobClient.Uri;
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

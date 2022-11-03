// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Microsoft.Extensions.Logging;
    using Quartz;

    [DisallowConcurrentExecution]
    public class GetBaseImageUriFolderJob : IJob
    {
        private const string ImageContainerName = "device-images";
        private readonly BlobServiceClient blobService;

        private readonly EnvVariableRegistry envVariableRegistry;

        private readonly ILogger<GetBaseImageUriFolderJob> logger;

        public GetBaseImageUriFolderJob(BlobServiceClient blobService,
            EnvVariableRegistry variableRegistry,
            ILogger<GetBaseImageUriFolderJob> logger)
        {
            this.blobService = blobService;
            this.envVariableRegistry = variableRegistry;
            this.logger = logger;

            var blobClient = this.blobService.GetBlobContainerClient(ImageContainerName);

            _ = blobClient.SetAccessPolicy(PublicAccessType.Blob);
            _ = blobClient.CreateIfNotExists();
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start get base image Uri folder job.");

                GetImageUriFolder();

                this.logger.LogInformation("End get base image Uri folder job.");

            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Get base image Uri folder job has failed.");
            }

            return Task.CompletedTask;
        }

        public void GetImageUriFolder()
        {
            this.envVariableRegistry.BaseImageFolderUri = this.blobService.GetBlobContainerClient(ImageContainerName).Uri;
        }
    }
}

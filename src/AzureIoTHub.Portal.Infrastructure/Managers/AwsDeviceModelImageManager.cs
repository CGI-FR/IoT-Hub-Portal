// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Managers
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Azure;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Options;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class AwsDeviceModelImageManager : IDeviceModelImageManager
    {
        private readonly ILogger<AwsDeviceModelImageManager> logger;
        private readonly ConfigHandler configHandler;
        private readonly IOptions<DeviceModelImageOptions> imageOptions;
        private readonly IAmazonS3 s3Client;

        public AwsDeviceModelImageManager(
            ILogger<AwsDeviceModelImageManager> logger,
            ConfigHandler configHandler,
            IOptions<DeviceModelImageOptions> options,
            IAmazonS3 s3Client)
        {
            this.logger = logger;
            this.configHandler = configHandler;
            this.imageOptions = options;
            this.s3Client = s3Client;
        }


        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, Stream stream)
        {
            this.logger.LogInformation($"Uploading Image to AWS S3 storage");

            var currentAssembly = Assembly.GetExecutingAssembly();
            var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{this.imageOptions.Value.DefaultImageName}");

            //Portal must be able to upload images to Amazon S3
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = deviceModelId,
                InputStream = stream??defaultImageStream,
                ContentType = "image/*",
                Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }
            };

            var putObjectResponse = await this.s3Client.PutObjectAsync(putObjectRequest);

            if (putObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                //Images on S3 are publicly accessible and read-only 
                var putAclRequest = new PutACLRequest
                {
                    BucketName = this.configHandler.AWSBucketName,
                    Key = deviceModelId,
                    CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                };
                var putACLResponse = await this.s3Client.PutACLAsync(putAclRequest);

                return putACLResponse.HttpStatusCode == System.Net.HttpStatusCode.OK
                    ? ComputeImageUrl(deviceModelId)
                    : throw new InternalServerErrorException("Error by setting the image access to public and read-only");
            }
            else
            {
                throw new InternalServerErrorException("Error by uploading the image in S3 Storage");
            }

        }

        public Uri ComputeImageUri(string deviceModelId)
        {
            throw new NotImplementedException();
        }

        public string ComputeImageUrl(string deviceModelId)
        {
            return $"https://{this.configHandler.AWSBucketName}.s3.{RegionEndpoint.GetBySystemName(this.configHandler.AWSRegion)}.amazonaws.com/{deviceModelId}";
        }
        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {

            this.logger.LogInformation($"Deleting image from AWS S3 storage");

            var deleteImageObject = new DeleteObjectRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = deviceModelId
            };
            try
            {
                _ = await this.s3Client.DeleteObjectAsync(deleteImageObject);

            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to delete the image from S3 storage.", e);
            }
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {
            this.logger.LogInformation($"Uploading Default Image to AWS S3 storage");
            var currentAssembly = Assembly.GetExecutingAssembly();
            var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{this.imageOptions.Value.DefaultImageName}");


            //Portal must be able to upload images to Amazon S3
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = deviceModelId,
                InputStream = defaultImageStream,
                ContentType = "image/*", // image content type
                Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }

            };

            var putObjectResponse = await this.s3Client.PutObjectAsync(putObjectRequest);

            if (putObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                //Images on S3 are publicly accessible and read-only 
                var putAclRequest = new PutACLRequest
                {
                    BucketName = this.configHandler.AWSBucketName,
                    Key = deviceModelId,
                    CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                };
                var putACLResponse = await this.s3Client.PutACLAsync(putAclRequest);

                return putACLResponse.HttpStatusCode == System.Net.HttpStatusCode.OK
                    ? ComputeImageUrl(deviceModelId)
                    : throw new InternalServerErrorException("Error by setting the image access to public and read-only");
            }
            else
            {
                throw new InternalServerErrorException("Error by uploading the image in S3 Storage");
            }

        }

        public async Task InitializeDefaultImageBlob()
        {

            this.logger.LogInformation($"Initializing default Image to AWS S3 storage");

            var currentAssembly = Assembly.GetExecutingAssembly();

            var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{this.imageOptions.Value.DefaultImageName}");
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = this.imageOptions.Value.DefaultImageName,
                InputStream = defaultImageStream,
                ContentType = "image/*", // image content type
                Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }

            };

            var putObjectResponse = await this.s3Client.PutObjectAsync(putObjectRequest);

            if (putObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                //Images on S3 are publicly accessible and read-only 
                var putAclRequest = new PutACLRequest
                {
                    BucketName = this.configHandler.AWSBucketName,
                    Key = this.imageOptions.Value.DefaultImageName,
                    CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                };
                var putACLResponse = await this.s3Client.PutACLAsync(putAclRequest);

                if (putACLResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InternalServerErrorException("Error by setting the image access to public and read-only");
                }
            }
            else
            {
                throw new InternalServerErrorException("Error by uploading the image in S3 Storage");
            }

        }

        public Task SyncImagesCacheControl()
        {
            /* We don't need an implementation of
             this mehod for AWS because new images will processed by the method SetDefaultImageToModel
             */
            throw new NotImplementedException();

        }
    }
}

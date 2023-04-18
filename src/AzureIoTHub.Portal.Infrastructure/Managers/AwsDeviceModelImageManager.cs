// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Managers
{
    using System;
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

            //Portal must be able to upload images to Amazon S3
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = deviceModelId,
                InputStream = stream,
                ContentType = "image/*",
                Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }
            };

            var putObjectResponse = await this.s3Client.PutObjectAsync(putObjectRequest);

            //Images on S3 are publicly accessible and read-only 
            var putAclRequest = new PutACLRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = deviceModelId,
                CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
            };

            _ = await this.s3Client.PutACLAsync(putAclRequest);

            if (putObjectResponse != null)
            {
                return ComputeImageUrl(deviceModelId);

            }
            else { throw new AmazonS3Exception("ObjectResponse should not be null"); }
        }

        public Uri ComputeImageUri(string deviceModelId)
        {
            return new Uri($"https://{this.configHandler.AWSBucketName}.s3.{RegionEndpoint.GetBySystemName(this.configHandler.AWSRegion)}.amazonaws.com/{deviceModelId}");
        }

        private string ComputeImageUrl(string deviceModelId)
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
                throw new InternalServerErrorException("Unable to delete the image from the blob storage.", e);
            }
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {
            this.logger.LogInformation($"Uploading Default Image to AWS S3 storage");

            //Portal must be able to upload images to Amazon S3
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = deviceModelId,
                FilePath = $"../Resources/{this.imageOptions.Value.DefaultImageName}",
                ContentType = "image/*", // image content type
                Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }

            };

            var putObjectResponse = await this.s3Client.PutObjectAsync(putObjectRequest);

            //Images on S3 are publicly accessible and read-only 
            var putAclRequest = new PutACLRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = deviceModelId,
                CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
            };

            _ = await this.s3Client.PutACLAsync(putAclRequest);

            return putObjectResponse.HttpStatusCode.ToString();

        }

        public async Task InitializeDefaultImageBlob()
        {

            this.logger.LogInformation($"Initializing default Image to AWS S3 storage");

            var putObjectRequest = new PutObjectRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = this.imageOptions.Value.DefaultImageName,
                FilePath = $"../Resources/{this.imageOptions.Value.DefaultImageName}",
                ContentType = "image/*" // image content type
            };

            _ = await this.s3Client.PutObjectAsync(putObjectRequest);

            //Images on S3 are publicly accessible and read-only 
            var putAclRequest = new PutACLRequest
            {
                BucketName = this.configHandler.AWSBucketName,
                Key = this.imageOptions.Value.DefaultImageName,
                CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
            };

            _ = await this.s3Client.PutACLAsync(putAclRequest);

        }

        public async Task SyncImagesCacheControl()
        {

            this.logger.LogInformation($"Synchronize Cache control images");

            var listImagesObjects = new ListObjectsRequest
            {
                BucketName = this.configHandler.AWSBucketName
            };

            //Get All images from AWS S3
            var response = await this.s3Client.ListObjectsAsync(listImagesObjects);
            foreach (var item in response.S3Objects)
            {
                var copyObjectRequest = new CopyObjectRequest
                {
                    SourceBucket = this.configHandler.AWSBucketName,
                    SourceKey = item.Key,
                    DestinationBucket = this.configHandler.AWSBucketName,
                    DestinationKey = item.Key,
                    Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }
                };
                _ = await this.s3Client.CopyObjectAsync(copyObjectRequest);
            }
        }
    }
}

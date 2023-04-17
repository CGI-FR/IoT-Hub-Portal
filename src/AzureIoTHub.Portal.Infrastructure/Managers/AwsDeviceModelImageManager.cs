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
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class AwsDeviceModelImageManager : IAwsDeviceModelImageManager
    {
        private readonly ILogger<AwsDeviceModelImageManager> logger;
        private readonly ConfigHandler configHandler;
        private readonly IOptions<DeviceModelImageOptions> imageOptions;

        public AwsDeviceModelImageManager(
            ILogger<AwsDeviceModelImageManager> logger,
            ConfigHandler configHandler,
            IOptions<DeviceModelImageOptions> options)
        {
            this.logger = logger;
            this.configHandler = configHandler;
            this.imageOptions = options;
        }


        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, IFormFile file, string bucketName)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(this.configHandler.AWSAccess, this.configHandler.AWSAccessSecret);
            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.configHandler.AWSRegion));

            this.logger.LogInformation($"Uploading Image to AWS S3 storage");

            PutObjectResponse putObjectResponse = null;
            //Portal must be able to upload images to Amazon S3
            if (file != null && file.Length > 0)
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = deviceModelId,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType, // image content type
                    Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }
                };

                putObjectResponse = await s3Client.PutObjectAsync(putObjectRequest);
            }


            //Images on S3 are publicly accessible and read-only 
            var putAclRequest = new PutACLRequest
            {
                BucketName = bucketName,
                Key = deviceModelId,
                CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
            };

            _ = await s3Client.PutACLAsync(putAclRequest);

            if (putObjectResponse != null)
            {
                return putObjectResponse.HttpStatusCode.ToString();

            }
            else { return ""; }
        }

        public Uri ComputeImageUri(string deviceModelId, string bucketName)
        {
            return new Uri($"https://{bucketName}.s3.{RegionEndpoint.GetBySystemName(this.configHandler.AWSRegion)}.amazonaws.com/{deviceModelId}");
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId, string bucketName)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(this.configHandler.AWSAccess, this.configHandler.AWSAccessSecret);
            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.configHandler.AWSRegion));

            this.logger.LogInformation($"Deleting image from AWS S3 storage");

            var deleteImageObject = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = deviceModelId
            };
            try
            {
                _ = await s3Client.DeleteObjectAsync(deleteImageObject);

            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to delete the image from the blob storage.", e);
            }
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId, string bucketName)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(this.configHandler.AWSAccess, this.configHandler.AWSAccessSecret);
            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.configHandler.AWSRegion));

            this.logger.LogInformation($"Uploading Default Image to AWS S3 storage");

            //Portal must be able to upload images to Amazon S3
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = deviceModelId,
                FilePath = $"../Resources/{this.imageOptions.Value.DefaultImageName}",
                ContentType = "image/png", // image content type
                Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }

            };

            var putObjectResponse = await s3Client.PutObjectAsync(putObjectRequest);

            //Images on S3 are publicly accessible and read-only 
            var putAclRequest = new PutACLRequest
            {
                BucketName = bucketName,
                Key = deviceModelId,
                CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
            };

            _ = await s3Client.PutACLAsync(putAclRequest);

            return putObjectResponse.HttpStatusCode.ToString();

        }

        public async Task InitializeDefaultImageBlob(string bucketName)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(this.configHandler.AWSAccess, this.configHandler.AWSAccessSecret);
            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.configHandler.AWSRegion));

            this.logger.LogInformation($"Initializing default Image to AWS S3 storage");

            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = this.imageOptions.Value.DefaultImageName,
                FilePath = $"../Resources/{this.imageOptions.Value.DefaultImageName}",
                ContentType = "image/png" // image content type
            };

            _ = await s3Client.PutObjectAsync(putObjectRequest);

            //Images on S3 are publicly accessible and read-only 
            var putAclRequest = new PutACLRequest
            {
                BucketName = bucketName,
                Key = this.imageOptions.Value.DefaultImageName,
                CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
            };

            _ = await s3Client.PutACLAsync(putAclRequest);

        }

        public async Task SyncImagesCacheControl(string bucketName)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(this.configHandler.AWSAccess, this.configHandler.AWSAccessSecret);
            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.configHandler.AWSRegion));

            this.logger.LogInformation($"Synchronize Cache control images");

            var listImagesObjects = new ListObjectsRequest
            {
                BucketName = bucketName
            };

            //Get All images from AWS S3
            var response = await s3Client.ListObjectsAsync(listImagesObjects);
            foreach (var item in response.S3Objects)
            {
                var copyObjectRequest = new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = item.Key,
                    DestinationBucket = bucketName,
                    DestinationKey = item.Key,
                    Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }
                };
                _ = await s3Client.CopyObjectAsync(copyObjectRequest);
            }
        }
    }
}

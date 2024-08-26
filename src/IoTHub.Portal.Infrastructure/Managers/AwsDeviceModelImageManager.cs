// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Managers
{
    using System;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Model;
    using IoTHub.Portal.Application.Managers;
    using Domain;
    using Domain.Exceptions;
    using Microsoft.Extensions.Logging;
    using Shared.Constants;

    public class AwsDeviceModelImageManager : IDeviceModelImageManager
    {
        private readonly ILogger<AwsDeviceModelImageManager> logger;
        private readonly ConfigHandler configHandler;
        private readonly IAmazonS3 s3Client;

        public AwsDeviceModelImageManager(
            ILogger<AwsDeviceModelImageManager> logger,
            ConfigHandler configHandler,
            IAmazonS3 s3Client)
        {
            this.logger = logger;
            this.configHandler = configHandler;
            this.s3Client = s3Client;
        }


        public Task<string> GetDeviceModelImageAsync(string deviceModelId)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, string file)
        {

            this.logger.LogInformation("Uploading Image to AWS S3 storage");

            try
            {
                //Portal must be able to upload images to Amazon S3
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = this.configHandler.AWSBucketName,
                    Key = deviceModelId,
                    ContentBody = file,
                    ContentType = "image/*",
                    Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }
                };

                _ = await this.s3Client.PutObjectAsync(putObjectRequest);

                try
                {
                    //Images on S3 are publicly accessible and read-only 
                    var putAclRequest = new PutACLRequest
                    {
                        BucketName = this.configHandler.AWSBucketName,
                        Key = deviceModelId,
                        CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                    };

                    _ = await this.s3Client.PutACLAsync(putAclRequest);

                    // TODO Return encoded image instead
                    return ComputeImageUri(deviceModelId).ToString();
                }
                catch (AmazonS3Exception e)
                {
                    throw new InternalServerErrorException("Unable to set the image access to public and read-only due to an error in Amazon S3 API.", e);
                }

            }
            catch (AmazonS3Exception e)
            {
                throw new InternalServerErrorException(" Unable to upload the image in S3 Bucket due to an error in Amazon S3 API.", e);
            }

        }

        public Uri ComputeImageUri(string deviceModelId)
        {
            var url = $"https://{this.configHandler.AWSBucketName}.s3.{this.configHandler.AWSRegion}.amazonaws.com/{deviceModelId}";
            return new Uri(url);
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {

            this.logger.LogInformation("Deleting image from AWS S3 storage");

            try
            {
                var deleteImageObject = new DeleteObjectRequest
                {
                    BucketName = this.configHandler.AWSBucketName,
                    Key = deviceModelId
                };

                _ = await this.s3Client.DeleteObjectAsync(deleteImageObject);

            }
            catch (AmazonS3Exception e)
            {
                throw new InternalServerErrorException("Unable to delete the image from S3 storage due to an error in Amazon S3 API.", e);
            }
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {

            this.logger.LogInformation("Uploading Default Image to AWS S3 storage");

            try
            {
                //Portal must be able to upload images to Amazon S3
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = this.configHandler.AWSBucketName,
                    Key = deviceModelId,
                    InputStream = DeviceModelImageOptions.DefaultImageStream,
                    ContentType = "image/*", // image content type
                    Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }
                };

                _ = await this.s3Client.PutObjectAsync(putObjectRequest);

                try
                {
                    //Images on S3 are publicly accessible and read-only 
                    var putAclRequest = new PutACLRequest
                    {
                        BucketName = this.configHandler.AWSBucketName,
                        Key = deviceModelId,
                        CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                    };
                    _ = await this.s3Client.PutACLAsync(putAclRequest);

                    // TODO Return encoded image instead
                    return ComputeImageUri(deviceModelId).ToString();
                }
                catch (AmazonS3Exception e)
                {
                    throw new InternalServerErrorException("Unable to set the image access to public and read-only due to an error in Amazon S3 API.", e);
                }
            }
            catch (AmazonS3Exception e)
            {
                throw new InternalServerErrorException("Unable to upload the image in S3 Bucket due to an error in Amazon S3 API.", e);
            }

        }

        public async Task InitializeDefaultImageBlob()
        {

            this.logger.LogInformation("Initializing default Image to AWS S3 storage");

            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = this.configHandler.AWSBucketName,
                    Key = DeviceModelImageOptions.DefaultImageName,
                    InputStream = DeviceModelImageOptions.DefaultImageStream,
                    ContentType = "image/*", // image content type
                    Headers = {CacheControl = $"max-age={this.configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }

                };

                _ = await this.s3Client.PutObjectAsync(putObjectRequest);

                try
                {
                    //Images on S3 are publicly accessible and read-only 
                    var putAclRequest = new PutACLRequest
                    {
                        BucketName = this.configHandler.AWSBucketName,
                        Key = DeviceModelImageOptions.DefaultImageName,
                        CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                    };

                    _ = await this.s3Client.PutACLAsync(putAclRequest);
                }
                catch (AmazonS3Exception e)
                {
                    throw new InternalServerErrorException("Unable to set the image access to public and read-only due to an error in Amazon S3 API.", e);
                }
            }
            catch (AmazonS3Exception e)
            {
                throw new InternalServerErrorException("Unable to upload the image in S3 Bucket due to an error in Amazon S3 API.", e);
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

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.AWS.Managers
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Model;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Options;
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
            imageOptions = options;
            this.s3Client = s3Client;
        }


        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, Stream stream)
        {

            logger.LogInformation($"Uploading Image to AWS S3 storage");

            try
            {
                //Portal must be able to upload images to Amazon S3
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = configHandler.AWSBucketName,
                    Key = deviceModelId,
                    InputStream = stream,
                    ContentType = "image/*",
                    Headers = { CacheControl = $"max-age={configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }
                };

                _ = await s3Client.PutObjectAsync(putObjectRequest);

                try
                {
                    //Images on S3 are publicly accessible and read-only 
                    var putAclRequest = new PutACLRequest
                    {
                        BucketName = configHandler.AWSBucketName,
                        Key = deviceModelId,
                        CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                    };

                    _ = await s3Client.PutACLAsync(putAclRequest);

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
            var url = $"https://{configHandler.AWSBucketName}.s3.{configHandler.AWSRegion}.amazonaws.com/{deviceModelId}";
            return new Uri(url);
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {

            logger.LogInformation($"Deleting image from AWS S3 storage");

            try
            {
                var deleteImageObject = new DeleteObjectRequest
                {
                    BucketName = configHandler.AWSBucketName,
                    Key = deviceModelId
                };

                _ = await s3Client.DeleteObjectAsync(deleteImageObject);

            }
            catch (AmazonS3Exception e)
            {
                throw new InternalServerErrorException("Unable to delete the image from S3 storage due to an error in Amazon S3 API.", e);
            }
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {

            logger.LogInformation($"Uploading Default Image to AWS S3 storage");

            var currentAssembly = Assembly.GetExecutingAssembly();
            var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{imageOptions.Value.DefaultImageName}");

            try
            {
                //Portal must be able to upload images to Amazon S3
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = configHandler.AWSBucketName,
                    Key = deviceModelId,
                    InputStream = defaultImageStream,
                    ContentType = "image/*", // image content type
                    Headers = { CacheControl = $"max-age={configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }

                };

                _ = await s3Client.PutObjectAsync(putObjectRequest);

                try
                {
                    //Images on S3 are publicly accessible and read-only 
                    var putAclRequest = new PutACLRequest
                    {
                        BucketName = configHandler.AWSBucketName,
                        Key = deviceModelId,
                        CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                    };
                    _ = await s3Client.PutACLAsync(putAclRequest);

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

            logger.LogInformation($"Initializing default Image to AWS S3 storage");

            var currentAssembly = Assembly.GetExecutingAssembly();

            var defaultImageStream = currentAssembly
                                            .GetManifestResourceStream($"{currentAssembly.GetName().Name}.Resources.{imageOptions.Value.DefaultImageName}");

            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = configHandler.AWSBucketName,
                    Key = imageOptions.Value.DefaultImageName,
                    InputStream = defaultImageStream,
                    ContentType = "image/*", // image content type
                    Headers = { CacheControl = $"max-age={configHandler.StorageAccountDeviceModelImageMaxAge}, must-revalidate" }

                };

                _ = await s3Client.PutObjectAsync(putObjectRequest);

                try
                {
                    //Images on S3 are publicly accessible and read-only 
                    var putAclRequest = new PutACLRequest
                    {
                        BucketName = configHandler.AWSBucketName,
                        Key = imageOptions.Value.DefaultImageName,
                        CannedACL = S3CannedACL.PublicRead // Set the object's ACL to public read
                    };

                    _ = await s3Client.PutACLAsync(putAclRequest);
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

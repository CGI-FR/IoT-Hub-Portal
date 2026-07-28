// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Managers
{
    /// <summary>
    /// Manages device model images by storing them as base64 strings directly in the database.
    /// Used when no Azure Storage Account is configured.
    /// </summary>
    public class DatabaseDeviceModelImageManager : IDeviceModelImageManager
    {
        private readonly ILogger<DatabaseDeviceModelImageManager> logger;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly IUnitOfWork unitOfWork;

        public DatabaseDeviceModelImageManager(
            ILogger<DatabaseDeviceModelImageManager> logger,
            IDeviceModelRepository deviceModelRepository,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.deviceModelRepository = deviceModelRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<string> GetDeviceModelImageAsync(string deviceModelId)
        {
            var deviceModel = await this.deviceModelRepository.GetByIdAsync(deviceModelId);

            if (deviceModel == null || string.IsNullOrEmpty(deviceModel.Image))
            {
                deviceModelId = deviceModelId.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
                this.logger.LogWarning("Image for device model {DeviceModelId} not found in database. Returning default image.", deviceModelId);
                return DeviceModelImageOptions.DefaultImage;
            }

            return deviceModel.Image;
        }

        public async Task<string> ChangeDeviceModelImageAsync(string deviceModelId, string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return await this.SetDefaultImageToModel(deviceModelId);
            }

            var deviceModel = await this.deviceModelRepository.GetByIdAsync(deviceModelId);

            if (deviceModel == null)
            {
                throw new IoTHub.Portal.Domain.Exceptions.ResourceNotFoundException($"The device model {deviceModelId} doesn't exist");
            }

            deviceModel.Image = file;

            this.deviceModelRepository.Update(deviceModel);
            await this.unitOfWork.SaveAsync();

            return file;
        }

        public async Task<string> SetDefaultImageToModel(string deviceModelId)
        {
            var deviceModel = await this.deviceModelRepository.GetByIdAsync(deviceModelId);

            if (deviceModel == null)
            {
                throw new IoTHub.Portal.Domain.Exceptions.ResourceNotFoundException($"The device model {deviceModelId} doesn't exist");
            }

            deviceModel.Image = DeviceModelImageOptions.DefaultImage;

            this.deviceModelRepository.Update(deviceModel);
            await this.unitOfWork.SaveAsync();

            return DeviceModelImageOptions.DefaultImage;
        }

        public async Task DeleteDeviceModelImageAsync(string deviceModelId)
        {
            var deviceModel = await this.deviceModelRepository.GetByIdAsync(deviceModelId);

            if (deviceModel == null)
            {
                return;
            }

            deviceModel.Image = null;

            this.deviceModelRepository.Update(deviceModel);
            await this.unitOfWork.SaveAsync();
        }

        public Task InitializeDefaultImageBlob()
        {
            // No-op: images are stored per device model in the database, there is no shared default blob to initialize.
            return Task.CompletedTask;
        }

        public Task SyncImagesCacheControl()
        {
            // No-op: cache control headers are only relevant when images are served from blob storage.
            return Task.CompletedTask;
        }
    }
}

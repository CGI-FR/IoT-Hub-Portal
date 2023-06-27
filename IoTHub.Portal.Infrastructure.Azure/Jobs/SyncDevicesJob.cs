// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Jobs
{
    using AutoMapper;
    using Domain;
    using Domain.Repositories;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Azure.Helpers;
    using IoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [DisallowConcurrentExecution]
    public class SyncDevicesJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<SyncDevicesJob> logger;

        private const string ModelId = "modelId";

        public SyncDevicesJob(IExternalDeviceService externalDeviceService,
            IDeviceModelRepository deviceModelRepository,
            ILorawanDeviceRepository lorawanDeviceRepository,
            IDeviceRepository deviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<SyncDevicesJob> logger)
        {
            this.externalDeviceService = externalDeviceService;
            this.deviceModelRepository = deviceModelRepository;
            this.lorawanDeviceRepository = lorawanDeviceRepository;
            this.deviceRepository = deviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync devices job");

                await SyncDevices();

                this.logger.LogInformation("End of sync devices job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync devices job has failed");
            }
        }

        private async Task SyncDevices()
        {
            var deviceTwins = await GetTwinDevices();

            foreach (var twin in deviceTwins)
            {
                if (!twin.Tags.Contains(ModelId))
                {
                    this.logger.LogInformation($"Cannot import device '{twin.DeviceId}' since it doesn't have a model identifier.");
                    continue;
                }

                var deviceModelId = twin.Tags[ModelId]?.ToString();

                var deviceModel = await this.deviceModelRepository.GetByIdAsync(deviceModelId);

                if (deviceModel == null)
                {
                    this.logger.LogInformation($"The device model {deviceModelId} does not exist, trying to import it.");

                    deviceModel = new DeviceModel
                    {
                        Id = deviceModelId!,
                        Name = deviceModelId!,
                        SupportLoRaFeatures = bool.Parse(DeviceHelper.RetrieveTagValue(twin, nameof(DeviceModel.SupportLoRaFeatures)) ?? "false")
                    };

                    await this.deviceModelRepository.InsertAsync(deviceModel);
                }

                if (deviceModel.SupportLoRaFeatures)
                {
                    await CreateOrUpdateLorawanDevice(twin);
                }
                else
                {
                    await CreateOrUpdateDevice(twin);
                }

                await this.unitOfWork.SaveAsync();
            }

            foreach (var item in (await this.deviceRepository.GetAllAsync()).Where(device => !deviceTwins.Exists(x => x.DeviceId == device.Id)))
            {
                this.deviceRepository.Delete(item.Id);
            }

            foreach (var item in (await this.lorawanDeviceRepository.GetAllAsync()).Where(lorawanDevice => !deviceTwins.Exists(x => x.DeviceId == lorawanDevice.Id)))
            {
                this.lorawanDeviceRepository.Delete(item.Id);
            }

            await this.unitOfWork.SaveAsync();
        }

        private async Task<List<Twin>> GetTwinDevices()
        {
            var twins = new List<Twin>();
            var continuationToken = string.Empty;

            int totalTwinDevices;
            do
            {
                var result = await this.externalDeviceService.GetAllDevice(continuationToken: continuationToken,excludeDeviceType: "LoRa Concentrator", pageSize: 100);
                twins.AddRange(result.Items);

                totalTwinDevices = result.TotalItems;
                continuationToken = result.NextPage;

            } while (totalTwinDevices > twins.Count);

            return twins;
        }

        private async Task CreateOrUpdateLorawanDevice(Twin twin)
        {
            var lorawanDevice = this.mapper.Map<LorawanDevice>(twin);

            var lorawanDeviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(lorawanDevice.Id, d => d.Tags);
            if (lorawanDeviceEntity == null)
            {
                await this.lorawanDeviceRepository.InsertAsync(lorawanDevice);
            }
            else
            {
                if (lorawanDeviceEntity.Version >= lorawanDevice.Version) return;

                foreach (var deviceTagEntity in lorawanDeviceEntity.Tags)
                {
                    this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
                }

                _ = this.mapper.Map(lorawanDevice, lorawanDeviceEntity);
                this.lorawanDeviceRepository.Update(lorawanDeviceEntity);
            }
        }

        private async Task CreateOrUpdateDevice(Twin twin)
        {
            var device = this.mapper.Map<Device>(twin);

            var deviceEntity = await this.deviceRepository.GetByIdAsync(device.Id, d => d.Tags);
            if (deviceEntity == null)
            {
                await this.deviceRepository.InsertAsync(device);
            }
            else
            {
                if (deviceEntity.Version >= device.Version) return;

                foreach (var deviceTagEntity in deviceEntity.Tags)
                {
                    this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
                }

                _ = this.mapper.Map(device, deviceEntity);
                this.deviceRepository.Update(deviceEntity);
            }
        }
    }
}

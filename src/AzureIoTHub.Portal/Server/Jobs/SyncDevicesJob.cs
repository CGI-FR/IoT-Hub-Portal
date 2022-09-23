// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Jobs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Quartz;

    [DisallowConcurrentExecution]
    public class SyncDevicesJob : IJob
    {
        private readonly IDeviceService deviceService;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly IDeviceRepository deviceRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<SyncDevicesJob> logger;

        public SyncDevicesJob(IDeviceService deviceService,
            IDeviceModelRepository deviceModelRepository,
            ILorawanDeviceRepository lorawanDeviceRepository,
            IDeviceRepository deviceRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<SyncDevicesJob> logger)
        {
            this.deviceService = deviceService;
            this.deviceModelRepository = deviceModelRepository;
            this.lorawanDeviceRepository = lorawanDeviceRepository;
            this.deviceRepository = deviceRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var twins = await GetAllDeviceTwin();

            foreach (var twin in twins)
            {
                var model = await this.deviceModelRepository.GetByIdAsync(twin.Tags["modelId"]?.ToString());

                if (model == null)
                {
                    continue;
                }

                if (model.SupportLoRaFeatures)
                {
                    try
                    {
                        var device = this.mapper.Map<LorawanDevice>(twin);

                        var lorawanDeviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(device.Id);
                        if (lorawanDeviceEntity == null)
                        {
                            await this.lorawanDeviceRepository.InsertAsync(device);
                        }
                        else
                        {
                            if (lorawanDeviceEntity.Version < device.Version)
                            {
                                _ = this.mapper.Map(device, lorawanDeviceEntity);
                                this.lorawanDeviceRepository.Update(lorawanDeviceEntity);
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        this.logger.LogWarning($"Error while attenpting to insert LoRa device {twin.DeviceId}.");
                    }
                }
                else
                {
                    var device = this.mapper.Map<Device>(twin);

                    var deviceEntity = await this.deviceRepository.GetByIdAsync(device.Id);
                    if (deviceEntity == null)
                    {
                        await this.deviceRepository.InsertAsync(device);
                    }
                    else
                    {
                        if (deviceEntity.Version < device.Version)
                        {
                            _ = this.mapper.Map(device, deviceEntity);
                            this.deviceRepository.Update(deviceEntity);
                        }
                    }
                }
            }

            // save entity
            await this.unitOfWork.SaveAsync();
        }

        private async Task<List<Twin>> GetAllDeviceTwin()
        {
            var twins = new List<Twin>();
            var continuationToken = string.Empty;

            int totalTwinDevices;
            do
            {
                var result = await this.deviceService.GetAllDevice(continuationToken: continuationToken,excludeDeviceType: "LoRa Concentrator", pageSize: 100);
                twins.AddRange(result.Items);

                totalTwinDevices = result.TotalItems;
                continuationToken = result.NextPage;

            } while (totalTwinDevices > twins.Count);

            return twins;
        }
    }
}

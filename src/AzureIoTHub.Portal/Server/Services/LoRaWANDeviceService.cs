// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Microsoft.Extensions.Logging;

    public class LoRaWANDeviceService : ILoRaWANDeviceService
    {
        private readonly ITableClientFactory tableClientFactory;
        private readonly IDeviceService deviceService;
        private readonly IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails> deviceTwinMapper;
        private readonly ILoraDeviceMethodManager loraDeviceMethodManager;
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;
        private readonly ILogger<LoRaWANDeviceService> logger;

        public LoRaWANDeviceService(ITableClientFactory tableClientFactory, IDeviceService deviceService, IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails> deviceTwinMapper, ILoraDeviceMethodManager loraDeviceMethodManager, IDeviceModelCommandMapper deviceModelCommandMapper, ILogger<LoRaWANDeviceService> logger)
        {
            this.tableClientFactory = tableClientFactory;
            this.deviceService = deviceService;
            this.deviceTwinMapper = deviceTwinMapper;
            this.loraDeviceMethodManager = loraDeviceMethodManager;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
            this.logger = logger;
        }

        public async Task ExecuteLoRaWANCommand(string deviceId, string commandId)
        {
            var twin = await this.deviceService.GetDeviceTwin(deviceId);
            var modelId = this.deviceTwinMapper.CreateDeviceDetails(twin, null).ModelId;
            TableEntity commandEntity;

            try
            {
                commandEntity = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'")
                .FirstOrDefault();

                if (commandEntity == null)
                {
                    throw new ResourceNotFoundException($"The LoRaWAN command {commandId} for the device {deviceId} cannot be found");
                }
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get the LoRaWAN command {commandId} for the device {deviceId}", e);
            }

            var deviceModelCommand = this.deviceModelCommandMapper.GetDeviceModelCommand(commandEntity);

            var result = await this.loraDeviceMethodManager.ExecuteLoRaDeviceMessage(deviceId, deviceModelCommand);

            if (!result.IsSuccessStatusCode)
            {
                logger.LogError($"{deviceId} - Execute command on device failed \n{(int)result.StatusCode} - {result.ReasonPhrase}\n{await result.Content.ReadAsStringAsync()}");

                throw new InternalServerErrorException($"Something went wrong when executing the command {deviceModelCommand.Name}.");
            }

            logger.LogInformation($"{deviceId} - Execute command: \n{(int)result.StatusCode} - {result.ReasonPhrase}\n{await result.Content.ReadAsStringAsync()}");
        }
    }
}

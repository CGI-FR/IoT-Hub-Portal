// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Shared.Models.v1._0;

    public class DevicePropertyService : IDevicePropertyService
    {
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;

        public DevicePropertyService(IExternalDeviceService externalDevicesService, IDeviceModelPropertiesService deviceModelPropertiesService)
        {
            this.externalDevicesService = externalDevicesService;
            this.deviceModelPropertiesService = deviceModelPropertiesService;
        }

        public async Task<IEnumerable<DevicePropertyValue>> GetProperties(string deviceId)
        {
            var device = await this.externalDevicesService.GetDeviceTwin(deviceId);

            if (device == null)
            {
                throw new ResourceNotFoundException($"Unable to find the device {deviceId}");
            }

            var modelId = DeviceHelper.RetrieveTagValue(device, nameof(DeviceDetails.ModelId));

            if (string.IsNullOrEmpty(modelId))
            {
                throw new ResourceNotFoundException($"Device {deviceId} has no modelId tag value");
            }

            IEnumerable<DeviceModelProperty> items;

            try
            {
                items = await this.deviceModelPropertiesService.GetModelProperties(modelId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get templates properties fro device with id {deviceId}: {e.Message}", e);
            }

            var result = new List<DevicePropertyValue>();
            JObject desiredPropertiesAsJson;
            JObject reportedPropertiesAsJson;

            try
            {
                desiredPropertiesAsJson = JObject.Parse(device.Properties.Desired.ToJson());
            }
            catch (JsonReaderException e)
            {
                throw new InternalServerErrorException($"Unable to read desired properties for device with id {deviceId}", e);
            }

            try
            {
                reportedPropertiesAsJson = JObject.Parse(device.Properties.Reported.ToJson());
            }
            catch (JsonReaderException e)
            {
                throw new InternalServerErrorException($"Unable to read reported properties for device with id {deviceId}", e);
            }

            foreach (var item in items)
            {
                var value = item.IsWritable ? desiredPropertiesAsJson.SelectToken(item.Name)?.Value<string>() :
                        reportedPropertiesAsJson.SelectToken(item.Name)?.Value<string>();

                result.Add(new DevicePropertyValue
                {
                    DisplayName = item.DisplayName,
                    IsWritable = item.IsWritable,
                    Name = item.Name,
                    PropertyType = item.PropertyType,
                    Value = value
                });
            }

            return result;
        }

        public async Task SetProperties(string deviceId, IEnumerable<DevicePropertyValue> devicePropertyValues)
        {
            var device = await this.externalDevicesService.GetDeviceTwin(deviceId);

            if (device == null)
            {
                throw new ResourceNotFoundException($"Unable to find the device {deviceId}");
            }

            var modelId = DeviceHelper.RetrieveTagValue(device, nameof(DeviceDetails.ModelId));

            if (string.IsNullOrEmpty(modelId))
            {
                throw new ResourceNotFoundException($"Device {deviceId} has no modelId tag value");
            }

            IEnumerable<DeviceModelProperty> items;

            try
            {
                items = await this.deviceModelPropertiesService.GetModelProperties(modelId);

            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get templates properties fro device with id {deviceId}: {e.Message}", e);
            }

            var desiredProperties = new Dictionary<string, object>();

            foreach (var item in items)
            {
                if (!item.IsWritable)
                {
                    continue;
                }

                _ = desiredProperties.TryAdd(item.Name, devicePropertyValues.FirstOrDefault(x => x.Name == item.Name)?.Value);
            }

            device.Properties.Desired = DeviceHelper.PropertiesWithDotNotationToTwinCollection(desiredProperties);

            _ = await this.externalDevicesService.UpdateDeviceTwin(device);
        }
    }
}

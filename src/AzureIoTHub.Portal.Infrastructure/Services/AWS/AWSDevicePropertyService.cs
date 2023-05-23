// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using Newtonsoft.Json;
    using Amazon.IotData.Model;
    using System.Text;
    using AzureIoTHub.Portal.Domain.Entities;
    using Newtonsoft.Json.Linq;
    using ResourceNotFoundException = Domain.Exceptions.ResourceNotFoundException;
    using AzureIoTHub.Portal.Domain.Repositories;
    using Azure;
    using AzureIoTHub.Portal.Application.Helpers;

    public class AWSDevicePropertyService : IDevicePropertyService
    {
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;
        private readonly IAWSExternalDeviceService externalDeviceService;
        private readonly IDeviceRepository deviceRepository;

        public AWSDevicePropertyService(IDeviceModelPropertiesService deviceModelPropertiesService
            , IAWSExternalDeviceService externalDeviceService
            , IDeviceRepository deviceRepository)
        {
            this.deviceModelPropertiesService = deviceModelPropertiesService;
            this.externalDeviceService = externalDeviceService;
            this.deviceRepository = deviceRepository;
        }

        public async Task<IEnumerable<DevicePropertyValue>> GetProperties(string deviceId)
        {
            var deviceDb = await this.deviceRepository.GetByIdAsync(deviceId);
            if (deviceDb == null)
            {
                throw new ResourceNotFoundException($"Unable to find the device {deviceId} in DB");
            }

            var deviceShadow = await this.externalDeviceService.GetDeviceShadow(deviceDb.Name);
            if (deviceShadow == null)
            {
                throw new ResourceNotFoundException($"Unable to find the device shadow {deviceDb.Name} in AWS");
            }

            IEnumerable<DeviceModelProperty> items;
            try
            {
                items = await this.deviceModelPropertiesService.GetModelProperties(deviceDb.DeviceModelId);
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException($"Unable to get templates properties fro device with id {deviceId}: {e.Message}", e);
            }

            var result = new List<DevicePropertyValue>();
            JObject desiredPropertiesAsJson;
            JObject reportedPropertiesAsJson;

            try
            {
                desiredPropertiesAsJson = JObject.Parse(JsonConvert.SerializeObject(AWSDeviceHelper.RetrieveDesiredProperties(deviceShadow)));
            }
            catch (JsonReaderException e)
            {
                throw new InternalServerErrorException($"Unable to read desired properties for device with id {deviceId}", e);
            }

            try
            {
                reportedPropertiesAsJson = JObject.Parse(JsonConvert.SerializeObject(AWSDeviceHelper.RetrieveReportedProperties(deviceShadow)));
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
            var device = await this.deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new ResourceNotFoundException($"Unable to find the device {deviceId} in DB");
            }

            IEnumerable<DeviceModelProperty> items;
            try
            {
                items = await this.deviceModelPropertiesService.GetModelProperties(device.DeviceModelId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get templates properties fro device with id {deviceId}: {e.Message}", e);
            }

            var desiredState = new Dictionary<string, string?>();
            foreach (var item in items)
            {
                if (!item.IsWritable)
                {
                    continue;
                }

                desiredState[item.Name] = devicePropertyValues.FirstOrDefault(x => x.Name == item.Name)?.Value;
            }

            var payload  = new
            {
                state = new
                {
                    desired = desiredState
                }
            };

            var shadowRequest = new UpdateThingShadowRequest
            {
                ThingName = device.Name,
                Payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)))
            };

            _ = await this.externalDeviceService.UpdateDeviceShadow(shadowRequest);
        }
    }
}

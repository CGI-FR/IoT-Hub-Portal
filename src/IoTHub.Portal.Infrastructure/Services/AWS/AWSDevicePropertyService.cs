// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Exceptions;
    using Newtonsoft.Json;
    using Amazon.IotData.Model;
    using System.Text;
    using IoTHub.Portal.Domain.Entities;
    using Newtonsoft.Json.Linq;
    using ResourceNotFoundException = Domain.Exceptions.ResourceNotFoundException;
    using IoTHub.Portal.Domain.Repositories;
    using Azure;
    using IoTHub.Portal.Application.Helpers;
    using Amazon.IoT;
    using Amazon.IotData;
    using Shared.Models.v1._0;

    public class AWSDevicePropertyService : IDevicePropertyService
    {
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;
        private readonly IDeviceRepository deviceRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonIotData amazonIotDataClient;

        public AWSDevicePropertyService(IDeviceModelPropertiesService deviceModelPropertiesService
            , IDeviceRepository deviceRepository
            , IAmazonIoT amazonIoTClient
            , IAmazonIotData amazonIotDataClient)
        {
            this.deviceModelPropertiesService = deviceModelPropertiesService;
            this.deviceRepository = deviceRepository;
            this.amazonIoTClient = amazonIoTClient;
            this.amazonIotDataClient = amazonIotDataClient;
        }

        public async Task<IEnumerable<DevicePropertyValue>> GetProperties(string deviceId)
        {
            var deviceDb = await this.deviceRepository.GetByIdAsync(deviceId);
            if (deviceDb == null)
            {
                throw new ResourceNotFoundException($"Unable to find the device {deviceId} in DB");
            }

            GetThingShadowResponse shadowResponse;
            try
            {
                shadowResponse = await this.amazonIotDataClient.GetThingShadowAsync(new GetThingShadowRequest
                {
                    ThingName = deviceDb.Name
                });
            }
            catch (AmazonIotDataException e)
            {
                throw new InternalServerErrorException($"Unable to get the thing shadow with device name : {deviceDb.Name} due to an error in the Amazon IoT API", e);
            }

            IEnumerable<DeviceModelProperty> items;
            try
            {
                items = await this.deviceModelPropertiesService.GetModelProperties(deviceDb.DeviceModelId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get templates properties for device with id {deviceId}: {e.Message}", e);
            }

            var result = new List<DevicePropertyValue>();
            JObject desiredPropertiesAsJson;
            JObject reportedPropertiesAsJson;

            try
            {
                desiredPropertiesAsJson = AWSDeviceHelper.RetrieveDesiredProperties(shadowResponse);
            }
            catch (JsonReaderException e)
            {
                throw new InternalServerErrorException($"Unable to read desired properties for device with id {deviceId}", e);
            }

            try
            {
                reportedPropertiesAsJson = AWSDeviceHelper.RetrieveReportedProperties(shadowResponse);
            }
            catch (JsonReaderException e)
            {
                throw new InternalServerErrorException($"Unable to read reported properties for device with id {deviceId}", e);
            }

            foreach (var item in items)
            {
                var value = item.IsWritable ? desiredPropertiesAsJson?.SelectToken(item.Name)?.Value<string>() :
                        reportedPropertiesAsJson?.SelectToken(item.Name)?.Value<string>();

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
                throw new InternalServerErrorException($"Unable to get templates properties for device with id {deviceId}: {e.Message}", e);
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

            try
            {
                _ = await this.amazonIotDataClient.UpdateThingShadowAsync(shadowRequest);
            }
            catch (AmazonIotDataException e)
            {
                throw new InternalServerErrorException($"Unable to create/update the thing shadow with device name : {device.Name} due to an error in the Amazon IoT API", e);
            }
        }
    }
}

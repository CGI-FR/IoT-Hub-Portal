// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.IotData;
    using Amazon.IotData.Model;
    using Azure;
    using AzureIoTHub.Portal;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;

    public class AWSExternalDeviceService : IExternalDeviceService
    {
        private readonly AmazonIoTClient amazonIotClient;
        private readonly AmazonIotDataClient amazonIotDataClient;

        public AWSExternalDeviceService(IServiceProvider serviceProvider)
        {
            this.amazonIotClient = serviceProvider.GetService<AmazonIoTClient>()!;
            this.amazonIotDataClient = serviceProvider.GetService<AmazonIotDataClient>()!;
        }

        public async Task<Object> CreateDeviceWithTwin(string deviceId, bool isEdge, Object twin, Object isEnabled)
        {
            try
            {
                var thingResponse = await this.amazonIotClient.CreateThingAsync((CreateThingRequest)twin);

                var shadowRequest = new UpdateThingShadowRequest
                {
                    ThingName = deviceId
                };

                return await this.amazonIotDataClient.UpdateThingShadowAsync(shadowRequest);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to create the device twin with id {deviceId}: {e.Message}", e);
            }
        }

        public async Task<Object> CreateNewTwinFromDeviceId(string deviceId)
        {
            try
            {
                var existingDevice = await this.GetDevice(deviceId);

                if (existingDevice != null)
                {
                    throw new DeviceAlreadyExistsException($"The device with ID {deviceId} already exists");
                }

                var shadowRequest = new UpdateThingShadowRequest
                {
                    ThingName = deviceId
                };

                return shadowRequest;
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to create the device twin with id {deviceId}: {e.Message}", e);
            }
        }

        public Task DeleteDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<CloudToDeviceMethodResult> ExecuteC2DMethod(string deviceId, CloudToDeviceMethod method)
        {
            throw new NotImplementedException();
        }

        public Task<CloudToDeviceMethodResult> ExecuteCustomCommandC2DMethod(string deviceId, string moduleName, CloudToDeviceMethod method)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationResult<Twin>> GetAllDevice(string? continuationToken = null, string? filterDeviceType = null, string? excludeDeviceType = null, string? searchText = null, bool? searchStatus = null, bool? searchState = null, Dictionary<string, string>? searchTags = null, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationResult<Twin>> GetAllEdgeDevice(string? continuationToken = null, string? searchText = null, bool? searchStatus = null, string? searchType = null, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAllGatewayID()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetConcentratorsCount()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetConnectedDevicesCount()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetConnectedEdgeDevicesCount()
        {
            throw new NotImplementedException();
        }

        public async Task<Object> GetDevice(string deviceId)
        {
            try
            {
                return await this.amazonIotClient.DescribeThingAsync(deviceId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get device with id {deviceId}", e);
            }
        }

        public Task<int> GetDevicesCount()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetDevicesToExport()
        {
            throw new NotImplementedException();
        }

        public Task<Twin> GetDeviceTwin(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<Twin> GetDeviceTwinWithEdgeHubModule(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<Twin> GetDeviceTwinWithModule(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<EnrollmentCredentials> GetEdgeDeviceCredentials(string edgeDeviceId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule edgeModule)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetEdgeDevicesCount()
        {
            throw new NotImplementedException();
        }

        public Task<EnrollmentCredentials> GetEnrollmentCredentials(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<ConfigItem> RetrieveLastConfiguration(Twin twin)
        {
            throw new NotImplementedException();
        }

        public Task<Device> UpdateDevice(Device device)
        {
            throw new NotImplementedException();
        }

        public Task<Twin> UpdateDeviceTwin(Twin twin)
        {
            throw new NotImplementedException();
        }
    }
}

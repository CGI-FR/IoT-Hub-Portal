// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Shared;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using ResourceAlreadyExistsException = Amazon.IoT.Model.ResourceAlreadyExistsException;
    using ResourceNotFoundException = Amazon.IoT.Model.ResourceNotFoundException;

    public class AwsExternalDeviceService : IExternalDeviceService
    {
        private readonly IMapper mapper;
        private readonly IAmazonIoT amazonIoTClient;

        public AwsExternalDeviceService(IMapper mapper,
            IAmazonIoT amazonIoTClient)
        {
            this.mapper = mapper;
            this.amazonIoTClient = amazonIoTClient;
        }

        public async Task<ExternalDeviceModelDto> CreateDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            try
            {
                var thingType = this.mapper.Map<CreateThingTypeRequest>(deviceModel);

                var createThingTypeRequest = this.mapper.Map<CreateThingTypeRequest>(thingType);

                createThingTypeRequest.Tags.Add(new Tag
                {
                    Key = "iotEdge",
                    Value = "False"
                });

                var response = await this.amazonIoTClient.CreateThingTypeAsync(createThingTypeRequest);
                await this.CreateDynamicGroupForThingType(response.ThingTypeName);

                deviceModel.Id = response.ThingTypeId;

                return deviceModel;
            }
            catch (ResourceAlreadyExistsException e)
            {
                throw new Domain.Exceptions.ResourceAlreadyExistsException($"Unable to create the device model {deviceModel.Name}: {e.Message}", e);
            }
        }

        public Task<BulkRegistryOperationResult> CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin, DeviceStatus isEnabled)
        {
            throw new NotImplementedException();
        }

        public Task<Twin> CreateNewTwinFromDeviceId(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            try
            {
                var deprecated = new DeprecateThingTypeRequest()
                {
                    ThingTypeName = deviceModel.Name,
                    UndoDeprecate = false
                };

                var response = await this.amazonIoTClient.DeprecateThingTypeAsync(deprecated);

                _ = await this.amazonIoTClient.DeleteDynamicThingGroupAsync(new DeleteDynamicThingGroupRequest
                {
                    ThingGroupName = deviceModel.Name
                });
            }
            catch (ResourceNotFoundException e)
            {
                throw new Domain.Exceptions.ResourceNotFoundException($"Unable to delete the device model {deviceModel.Name}: {e.Message}", e);
            }
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

        public Task<Device> GetDevice(string deviceId)
        {
            throw new NotImplementedException();
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

        private async Task CreateDynamicGroupForThingType(string thingTypeName)
        {
            try
            {
                var dynamicThingGroup = new DescribeThingGroupRequest
                {
                    ThingGroupName = thingTypeName
                };

                _ = await this.amazonIoTClient.DescribeThingGroupAsync(dynamicThingGroup);
            }
            catch (ResourceNotFoundException)
            {
                _ = await this.amazonIoTClient.CreateDynamicThingGroupAsync(new CreateDynamicThingGroupRequest
                {
                    ThingGroupName = thingTypeName,
                    QueryString = $"thingTypeName: {thingTypeName}"
                });
            }
        }
    }
}

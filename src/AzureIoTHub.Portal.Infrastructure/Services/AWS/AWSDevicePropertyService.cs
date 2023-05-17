// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Helpers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using Amazon.IotData.Model;
    using System.Text;
    using ResourceNotFoundException = Domain.Exceptions.ResourceNotFoundException;

    public class AWSDevicePropertyService : IDevicePropertyService
    {
        private readonly IAWSExternalDeviceService externalDevicesService;
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;
        private readonly IAWSExternalDeviceService externalDeviceService;

        public AWSDevicePropertyService(IAWSExternalDeviceService externalDevicesService
            , IDeviceModelPropertiesService deviceModelPropertiesService
            , IAWSExternalDeviceService externalDeviceService)
        {
            this.externalDevicesService = externalDevicesService;
            this.deviceModelPropertiesService = deviceModelPropertiesService;
            this.externalDeviceService = externalDeviceService;
        }

        public Task<IEnumerable<DevicePropertyValue>> GetProperties(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task SetProperties(string deviceId, IEnumerable<DevicePropertyValue> devicePropertyValues)
        {
            throw new NotImplementedException();
        }
    }
}

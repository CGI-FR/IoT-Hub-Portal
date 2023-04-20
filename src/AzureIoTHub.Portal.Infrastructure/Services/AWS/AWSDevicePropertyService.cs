// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Models.v10;

    public class AWSDevicePropertyService : IDevicePropertyService
    {
        private readonly IExternalDeviceService externalDevicesService;

        public AWSDevicePropertyService(IExternalDeviceService externalDevicesService)
        {
            this.externalDevicesService = externalDevicesService;
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

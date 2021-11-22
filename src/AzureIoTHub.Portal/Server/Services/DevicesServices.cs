// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Interfaces;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    public class DevicesServices : IDevicesService
    {
        private readonly RegistryManager registryManager;

        public DevicesServices(
            RegistryManager registryManager)
        {
            this.registryManager = registryManager;
        }

        public BulkRegistryOperationResult CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string deviceId)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Twin> GetAllEdgeDevice()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Twin> GetAllEdgeDeviceWithTags()
        {
            throw new System.NotImplementedException();
        }

        public Twin GetDeviceTwin(string deviceId)
        {
            throw new System.NotImplementedException();
        }

        public Twin GetDeviceWithModule(string deviceId)
        {
            throw new System.NotImplementedException();
        }

        public Device UpdateDevice(Device device)
        {
            throw new System.NotImplementedException();
        }

        public Twin UpdateDeviceTwin(string deviceId, Twin twin)
        {
            throw new System.NotImplementedException();
        }
    }
}

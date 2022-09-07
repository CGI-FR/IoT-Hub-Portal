// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using AzureIoTHub.Portal.Server.Factories;

    public class DeviceConfigurationsService : IDeviceConfigurationsService
    {
        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly IConfigService configService;

        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        public DeviceConfigurationsService(IConfigService configService, ITableClientFactory tableClientFactory)
        {
            this.configService = configService;
            this.tableClientFactory = tableClientFactory;
        }
    }
}

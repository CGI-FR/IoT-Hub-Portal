// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Extensions.Logging;

    public class ExportManager : IExportManager
    {
        private readonly PortalDbContext portalDbContext;
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceTagService deviceTagService;
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;
        private readonly ILogger<ExportManager> log;

        public ExportManager(PortalDbContext portalDbContext,
                                IExternalDeviceService externalDevicesService,
                                IDeviceTagService deviceTagService,
                                IDeviceModelPropertiesService deviceModelPropertiesService,
                                ILogger<ExportManager> log)
        {
            this.portalDbContext = portalDbContext;
            this.externalDevicesService = externalDevicesService;
            this.deviceTagService = deviceTagService;
            this.deviceModelPropertiesService = deviceModelPropertiesService;
            this.log = log;
        }


        public async Task<Stream> ExportDeviceList(bool isLoRaSupported)
        {
            var list = await this.externalDevicesService.GetDevicesToExport();
            var tags = this.deviceTagService.GetAllTagsNames() as List<string>;
            if (this.deviceModelPropertiesService.GetAllPropertiesNames() is not List<string> properties)
            {
                properties = new List<string>();
            }

            if (isLoRaSupported)
            {
                properties.AddRange(new List<string>()
                {
                    "AppKey",
                    "AppEUI",
                    "AppSKey",
                    "NwkSKey",
                    "DevAddr",
                    "GatewayID",
                    "Downlink",
                    "ClassType",
                    "PreferredWindow",
                    "Deduplication",
                    "RX1DROffset",
                    "RX2DataRate",
                    "RXDelay",
                    "ABPRelaxMode",
                    "SensorDecoder",
                    "FCntUpStart",
                    "FCntDownStart",
                    "FCntResetCounter",
                    "Supports32BitFCnt",
                    "KeepAliveTimeout"
                });
            }

            var stringBuilder = new StringBuilder("Id,Name,DeviceModelId");
            _ = stringBuilder.Append(",TAG:");
            _ = stringBuilder.AppendJoin(",TAG:", tags);
            _ = stringBuilder.Append(",PROPERTY:");
            _ = stringBuilder.AppendJoin(",PROPERTY:", properties);

            foreach (var item in list)
            {
                var deviceObject = JsonNode.Parse(item)!;

                _ = stringBuilder.Append(CultureInfo.InvariantCulture, $"\n{deviceObject["deviceId"]!},{deviceObject["tags"]["deviceName"]!},{deviceObject["tags"]["modelId"]!}");

                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        _ = stringBuilder.Append(CultureInfo.InvariantCulture, $",{deviceObject!["tags"][tag]}");
                    }
                }

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        _ = stringBuilder.Append(CultureInfo.InvariantCulture, $",{deviceObject!["desired"][property]}");
                    }
                }
            }

            var textContent = stringBuilder.ToString();

            var textAsBytes = Encoding.Unicode.GetBytes(textContent);
            var stream = new MemoryStream(textAsBytes);
            return stream;
        }
    }
}

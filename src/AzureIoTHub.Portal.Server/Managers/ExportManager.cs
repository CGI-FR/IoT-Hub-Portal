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
    using AzureIoTHub.Portal.Domain.Options;
    using AzureIoTHub.Portal.Server.Services;
    using CsvHelper;
    using Microsoft.Extensions.Options;

    public class ExportManager : IExportManager
    {
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceTagService deviceTagService;
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;
        private readonly IOptions<LoRaWANOptions> loRaWANOptions;

        public ExportManager(IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            IDeviceModelPropertiesService deviceModelPropertiesService,
            IOptions<LoRaWANOptions> loRaWANOptions)
        {
            this.externalDevicesService = externalDevicesService;
            this.deviceTagService = deviceTagService;
            this.deviceModelPropertiesService = deviceModelPropertiesService;
            this.loRaWANOptions = loRaWANOptions;
        }

        public async Task ExportDeviceList(Stream stream)
        {
            var list = await this.externalDevicesService.GetDevicesToExport();
            var tags = this.deviceTagService.GetAllTagsNames();
            var properties = new List<string>(this.deviceModelPropertiesService.GetAllPropertiesNames());

            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

            if (loRaWANOptions.Value.Enabled)
            {
                properties.AddRange(new[] {
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

            using var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture, leaveOpen: true);

            csvWriter.WriteField("Id");
            csvWriter.WriteField("Name");
            csvWriter.WriteField("ModelId");

            foreach (var tag in tags)
            {
                csvWriter.WriteField(string.Format(CultureInfo.InvariantCulture, $"TAG:{tag}"));
            }

            foreach (var property in properties)
            {
                csvWriter.WriteField(string.Format(CultureInfo.InvariantCulture, $"PROPERTY:{property}"));
            }

            await csvWriter.NextRecordAsync();

            foreach (var item in list)
            {
                var deviceObject = JsonNode.Parse(item)!;

                csvWriter.WriteField(deviceObject["deviceId"].ToString(), true);
                csvWriter.WriteField(deviceObject["tags"]["deviceName"]?.ToString(), true);
                csvWriter.WriteField(deviceObject["tags"]["modelId"]);

                foreach (var tag in tags)
                {
                    csvWriter.WriteField(string.Format(CultureInfo.InvariantCulture, $"{deviceObject!["tags"][tag]}"));
                }

                foreach (var property in properties)
                {
                    csvWriter.WriteField(string.Format(CultureInfo.InvariantCulture, $"{deviceObject!["desired"][property]}"));
                }

                await csvWriter.NextRecordAsync();
            }

            await csvWriter.FlushAsync();
        }
    }
}

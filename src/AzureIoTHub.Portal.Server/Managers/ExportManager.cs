// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Options;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Services;
    using CsvHelper;
    using Microsoft.Extensions.Options;

    public class ExportManager : IExportManager
    {
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceService<DeviceDetails> deviceService;
        private readonly IDeviceService<LoRaDeviceDetails> loraDeviceService;
        private readonly IDeviceTagService deviceTagService;
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;
        private readonly IOptions<LoRaWANOptions> loRaWANOptions;

        public ExportManager(IExternalDeviceService externalDevicesService,
            IDeviceService<DeviceDetails> deviceService,
            IDeviceService<LoRaDeviceDetails> loraDeviceService,
            IDeviceTagService deviceTagService,
            IDeviceModelPropertiesService deviceModelPropertiesService,
            IOptions<LoRaWANOptions> loRaWANOptions)
        {
            this.externalDevicesService = externalDevicesService;
            this.deviceService = deviceService;
            this.loraDeviceService = loraDeviceService;
            this.deviceTagService = deviceTagService;
            this.deviceModelPropertiesService = deviceModelPropertiesService;
            this.loRaWANOptions = loRaWANOptions;
        }

        public async Task ExportDeviceList(Stream stream)
        {
            var list = await this.externalDevicesService.GetDevicesToExport();
            var tags = GetTagsToExport();
            var properties = GetPropertiesToExport();

            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

            using var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture, leaveOpen: true);

            WriteHeader(tags, properties, csvWriter);

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

        public async Task ExportTemplateFile(Stream stream)
        {
            var tags = new List<string>(this.deviceTagService.GetAllTagsNames());
            var properties = GetPropertiesToExport();

            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

            using var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture, leaveOpen: true);

            WriteHeader(tags, properties, csvWriter);

            await csvWriter.FlushAsync();
        }

        private List<string> GetPropertiesToExport()
        {
            var properties = new List<string>(this.deviceModelPropertiesService.GetAllPropertiesNames());

            if (this.loRaWANOptions.Value.Enabled)
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

            return properties;
        }

        private List<string> GetTagsToExport()
        {
            var tags = new List<string>(this.deviceTagService.GetAllTagsNames());

            if (this.loRaWANOptions.Value.Enabled)
            {
                tags.Add("supportLoRaFeatures");
            }

            return tags;
        }

        private static void WriteHeader(List<string> tags, List<string> properties, CsvWriter csvWriter)
        {
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
        }

        public async Task ImportDeviceList(Stream stream)
        {
            try
            {
                var tags = GetTagsToExport();
                var properties = new List<string>(this.deviceModelPropertiesService.GetAllPropertiesNames());

                using var reader = new StreamReader(stream);
                using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                _ = csvReader.Read();
                _ = csvReader.ReadHeader();
                var header = csvReader.HeaderRecord;

                while (csvReader.Read())
                {
                    var isLora = csvReader.GetField("TAG:supportLoRaFeatures");

                    var deviceTags = new Dictionary<string,string>();
                    var deviceProperties = new Dictionary<string,string>();
                    foreach (var tag in tags)
                    {
                        deviceTags.Add(tag, csvReader.GetField($"TAG:{tag}"));
                    }

                    foreach (var property in properties)
                    {
                        deviceProperties.Add(property, csvReader.GetField($"PROPERTY:{property}"));
                    }

                    if (isLora == "true")
                    {
                        var newDevice = new LoRaDeviceDetails()
                        {
                            DeviceID = csvReader.GetField("Id"),
                            DeviceName = csvReader.GetField("Name"),
                            ModelId = csvReader.GetField("ModelId"),
                            AppKey = csvReader.GetField("PROPERTY:AppKey"),
                            AppEUI = csvReader.GetField("PROPERTY:AppEUI"),
                            AppSKey = csvReader.GetField("PROPERTY:AppSKey"),
                            NwkSKey= csvReader.GetField("PROPERTY:NwkSKey"),
                            DevAddr= csvReader.GetField("PROPERTY:DevAddr"),
                            GatewayID= csvReader.GetField("PROPERTY:GatewayID"),
                            Downlink= csvReader.GetField<bool?>("PROPERTY:Downlink"),
                            ClassType = Enum.TryParse<ClassType>(csvReader.GetField("PROPERTY:ClassType"), out var classType) ? classType : ClassType.A,
                            PreferredWindow= csvReader.GetField<int>("PROPERTY:PreferredWindow"),
                            Deduplication = Enum.TryParse<DeduplicationMode>(csvReader.GetField("PROPERTY:Deduplication"), out var deduplication) ? deduplication : DeduplicationMode.None,
                            RX1DROffset= csvReader.GetField<int?>("PROPERTY:RX1DROffset"),
                            RX2DataRate= csvReader.GetField<int?>("PROPERTY:RX2DataRate"),
                            RXDelay= csvReader.GetField<int?>("PROPERTY:RXDelay"),
                            ABPRelaxMode= csvReader.GetField<bool?>("PROPERTY:ABPRelaxMode"),
                            SensorDecoder= csvReader.GetField("PROPERTY:SensorDecoder"),
                            FCntUpStart= csvReader.GetField<int?>("PROPERTY:FCntUpStart"),
                            FCntDownStart= csvReader.GetField<int?>("PROPERTY:FCntDownStart"),
                            FCntResetCounter= csvReader.GetField<int?>("PROPERTY:FCntResetCounter"),
                            Supports32BitFCnt= csvReader.GetField<bool?>("PROPERTY:Supports32BitFCnt"),
                            KeepAliveTimeout= csvReader.GetField<int?>("PROPERTY:KeepAliveTimeout"),
                            Tags = deviceTags
                        };

                        _ = await this.loraDeviceService.CreateDevice(newDevice);
                    }
                    else
                    {
                        var newDevice = new DeviceDetails()
                        {
                            DeviceID = csvReader.GetField("Id"),
                            DeviceName = csvReader.GetField("Name"),
                            ModelId = csvReader.GetField("ModelId"),
                            Tags = deviceTags
                        };

                        _ = await this.deviceService.CreateDevice(newDevice);
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

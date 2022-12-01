// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Options;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Services;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    public class ExportManager : IExportManager
    {
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceService<DeviceDetails> deviceService;
        private readonly IDeviceService<LoRaDeviceDetails> loraDeviceService;
        private readonly IDeviceTagService deviceTagService;
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;
        private readonly IDevicePropertyService devicePropertyService;
        private readonly IOptions<LoRaWANOptions> loRaWANOptions;

        public ExportManager(IExternalDeviceService externalDevicesService,
            IDeviceService<DeviceDetails> deviceService,
            IDeviceService<LoRaDeviceDetails> loraDeviceService,
            IDeviceTagService deviceTagService,
            IDeviceModelPropertiesService deviceModelPropertiesService,
            IDevicePropertyService devicePropertyService,
            IOptions<LoRaWANOptions> loRaWANOptions)
        {
            this.externalDevicesService = externalDevicesService;
            this.deviceService = deviceService;
            this.loraDeviceService = loraDeviceService;
            this.deviceTagService = deviceTagService;
            this.deviceModelPropertiesService = deviceModelPropertiesService;
            this.devicePropertyService = devicePropertyService;
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

        public async Task<string> ImportDeviceList(Stream stream)
        {
            var errorReport = new List<string>();

            var tags = GetTagsToExport();
            using var reader = new StreamReader(stream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            };
            using var csvReader = new CsvReader(reader, config);

            _ = csvReader.Read();
            _ = csvReader.ReadHeader();

            if (csvReader.HeaderRecord.Length < 3)
            {
                throw new InternalServerErrorException("Invalid file format: The submitted file should be a comma-separated values (CSV) file. A template file showing the mandatory fields is available to download on the portal.");
            }
            var lineNumber=0;
            while (csvReader.Read())
            {
                lineNumber++;

                if (!csvReader.TryGetField<string>("Id", out var deviceId) || deviceId.IsNullOrEmpty())
                {
                    errorReport.Add($"<b>Error</b> occured while processing device on <b>line {lineNumber}</b>: The parameter deviceId cannot be null or empty. Device was not imported.");
                    continue;
                }

                if (!csvReader.TryGetField<string>("Name", out var deviceName) || deviceName.IsNullOrEmpty())
                {
                    errorReport.Add($"<b>Error</b> occured while processing device on <b>line {lineNumber}</b>: The parameter deviceName cannot be null or empty. Device was not imported.");

                    continue;
                }

                if (!csvReader.TryGetField<string>("ModelId", out var modelId) || modelId.IsNullOrEmpty())
                {
                    errorReport.Add($"<b>Error</b> occured while processing device on <b>line {lineNumber}</b>: The parameter modelId cannot be null or empty. Device was not imported.");
                    continue;
                }

                var isLoRa = bool.TryParse(csvReader.GetField("TAG:supportLoRaFeatures"), out var supportLoRaFeatures) && supportLoRaFeatures;

                var deviceTags = new Dictionary<string,string>();

                foreach (var tag in tags)
                {
                    if (csvReader.TryGetField<string>($"TAG:{tag}", out var tagValue))
                    {
                        deviceTags.Add(tag, tagValue);
                    }
                }

                if (this.loRaWANOptions.Value.Enabled && isLoRa)
                {
                    try
                    {
                        var newDevice = new LoRaDeviceDetails()
                        {
                            DeviceID = deviceId,
                            DeviceName = deviceName,
                            ModelId = modelId,
                            AppKey = csvReader.TryGetField<string>("PROPERTY:AppKey", out var appKey) ? appKey : string.Empty,
                            AppEUI = csvReader.TryGetField<string>("PROPERTY:AppEUI", out var appEUI) ? appEUI : string.Empty,
                            AppSKey = csvReader.TryGetField<string>("PROPERTY:AppSKey", out var appSKey) ? appSKey : string.Empty,
                            NwkSKey = csvReader.TryGetField<string>("PROPERTY:NwkSKey", out var nwkSKey) ? nwkSKey : string.Empty,
                            DevAddr = csvReader.TryGetField<string>("PROPERTY:DevAddr", out var devAddr) ? devAddr : string.Empty,
                            GatewayID = csvReader.TryGetField<string>("PROPERTY:GatewayID", out var gatewayID) ? gatewayID : string.Empty,
                            Downlink = csvReader.TryGetField<bool?>("PROPERTY:Downlink", out var downlink) ? downlink : null,
                            ClassType = Enum.TryParse<ClassType>(csvReader.GetField("PROPERTY:ClassType"), out var classType) ? classType : ClassType.A,
                            PreferredWindow = csvReader.TryGetField<int>("PROPERTY:PreferredWindow", out var preferredWindow) ? preferredWindow : 1,
                            Deduplication = Enum.TryParse<DeduplicationMode>(csvReader.GetField("PROPERTY:Deduplication"), out var deduplication) ? deduplication : DeduplicationMode.Drop,
                            RX1DROffset = csvReader.TryGetField<int?>("PROPERTY:RX1DROffset", out var rX1DROffset) ? rX1DROffset : null,
                            RX2DataRate = csvReader.TryGetField<int?>("PROPERTY:RX1DROffset", out var rX2DataRate) ? rX2DataRate : null,
                            RXDelay = csvReader.TryGetField<int?>("PROPERTY:RXDelay", out var rXDelay) ? rXDelay : null,
                            ABPRelaxMode = csvReader.TryGetField<bool?>("PROPERTY:ABPRelaxMode", out var aBPRelaxMode) ? aBPRelaxMode : null,
                            SensorDecoder = csvReader.TryGetField<string>("PROPERTY:SensorDecoder", out var sensorDecoder) ? sensorDecoder : string.Empty,
                            FCntUpStart = csvReader.TryGetField<int?>("PROPERTY:FCntUpStart", out var fCntUpStart) ? fCntUpStart : null,
                            FCntDownStart = csvReader.TryGetField<int?>("PROPERTY:FCntDownStart", out var fCntDownStart) ? fCntDownStart : null,
                            FCntResetCounter = csvReader.TryGetField<int?>("PROPERTY:FCntResetCounter", out var fCntResetCounter) ? fCntResetCounter : null,
                            Supports32BitFCnt = csvReader.TryGetField<bool?>("PROPERTY:Supports32BitFCnt", out var supports32BitFCnt) ? supports32BitFCnt : null,
                            KeepAliveTimeout = csvReader.TryGetField<int?>("PROPERTY:KeepAliveTimeout", out var keepAliveTimeout) ? keepAliveTimeout : null,
                            IsEnabled = true,
                            Tags = deviceTags
                        };

                        _ = await this.loraDeviceService.CheckIfDeviceExists(newDevice.DeviceID)
                            ? await this.loraDeviceService.UpdateDevice(newDevice)
                            : await this.loraDeviceService.CreateDevice(newDevice);
                    }
                    catch (Exception e)
                    {
                        errorReport.Add($"<b>Error</b> occured while processing device {deviceId} on <b>line {lineNumber}</b>: {e.Message}. Device was not imported.");
                    }

                }
                else
                {
                    try
                    {
                        var properties = await this.deviceModelPropertiesService.GetModelProperties(modelId);
                        var deviceProperties = new List<DevicePropertyValue>();

                        foreach (var property in properties)
                        {
                            deviceProperties.Add(new DevicePropertyValue()
                            {
                                Name = property.Name,
                                PropertyType = property.PropertyType,
                                Value = csvReader.GetField($"PROPERTY:{property.Name}")
                            });
                        }

                        var newDevice = new DeviceDetails()
                        {
                            DeviceID = deviceId,
                            DeviceName = deviceName,
                            ModelId = modelId,
                            IsEnabled = true,
                            Tags = deviceTags
                        };

                        _ = await this.deviceService.CheckIfDeviceExists(newDevice.DeviceID)
                            ? await this.deviceService.UpdateDevice(newDevice)
                            : await this.deviceService.CreateDevice(newDevice);

                        await this.devicePropertyService.SetProperties(deviceId, deviceProperties);
                    }

                    catch (Exception e)
                    {
                        errorReport.Add($"<b>Error</b> occured while processing device {deviceId} on <b>line {lineNumber}</b>: {e.Message}. Device was not imported.");
                    }
                }
            }

            return JsonSerializer.Serialize(errorReport.ToArray());
        }
    }
}

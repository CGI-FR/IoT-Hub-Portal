// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Options;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Shared.Models.v10;
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

        private const string TagPrefix = "TAG";
        private const string PropertyPrefix = "PROPERTY";

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
                    nameof(LoRaDeviceDetails.AppKey),
                    nameof(LoRaDeviceDetails.AppEUI),
                    nameof(LoRaDeviceDetails.AppSKey),
                    nameof(LoRaDeviceDetails.NwkSKey),
                    nameof(LoRaDeviceDetails.DevAddr),
                    nameof(LoRaDeviceDetails.GatewayID),
                    nameof(LoRaDeviceDetails.Downlink),
                    nameof(LoRaDeviceDetails.ClassType),
                    nameof(LoRaDeviceDetails.PreferredWindow),
                    nameof(LoRaDeviceDetails.Deduplication),
                    nameof(LoRaDeviceDetails.RX1DROffset),
                    nameof(LoRaDeviceDetails.RX2DataRate),
                    nameof(LoRaDeviceDetails.RXDelay),
                    nameof(LoRaDeviceDetails.ABPRelaxMode),
                    nameof(LoRaDeviceDetails.SensorDecoder),
                    nameof(LoRaDeviceDetails.FCntUpStart),
                    nameof(LoRaDeviceDetails.FCntDownStart),
                    nameof(LoRaDeviceDetails.FCntResetCounter),
                    nameof(LoRaDeviceDetails.Supports32BitFCnt),
                    nameof(LoRaDeviceDetails.KeepAliveTimeout)
                });
            }

            return properties;
        }

        private List<string> GetTagsToExport()
        {
            var tags = new List<string>(this.deviceTagService.GetAllTagsNames());

            if (this.loRaWANOptions.Value.Enabled)
                tags.Add("supportLoRaFeatures");

            return tags;
        }

        private static void WriteHeader(List<string> tags, List<string> properties, CsvWriter csvWriter)
        {
            csvWriter.WriteField("Id");
            csvWriter.WriteField("Name");
            csvWriter.WriteField("ModelId");

            foreach (var tag in tags)
            {
                csvWriter.WriteField(string.Format(CultureInfo.InvariantCulture, $"{TagPrefix}:{tag}"));
            }

            foreach (var property in properties)
            {
                csvWriter.WriteField(string.Format(CultureInfo.InvariantCulture, $"{PropertyPrefix}:{property}"));
            }
        }

        public async Task<IEnumerable<ImportResultLine>> ImportDeviceList(Stream stream)
        {
            var report = new List<ImportResultLine>();

            using var reader = new StreamReader(stream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            };

            using var csvReader = new CsvReader(reader, config);

            _ = csvReader.Read();
            _ = csvReader.ReadHeader();

            if (csvReader.HeaderRecord.Length < 3)
                throw new InternalServerErrorException("Invalid file format: The submitted file should be a comma-separated values (CSV) file. A template file showing the mandatory fields is available to download on the portal.");

            var lineNumber = 0;

            var tags = GetTagsToExport();

            while (csvReader.Read())
            {
                lineNumber++;

                string deviceId = null;
                string deviceName = null;
                string modelId = null;

                if (!TryReadMandatoryFields(csvReader, lineNumber, ref deviceId, ref deviceName, ref modelId, ref report))
                    continue;

                try
                {
                    var isLoRa = bool.TryParse(csvReader.GetField($"{TagPrefix}:supportLoRaFeatures"), out var supportLoRaFeatures) && supportLoRaFeatures;

                    var deviceTags = ReadTags(csvReader, tags);

                    if (this.loRaWANOptions.Value.Enabled && isLoRa)
                    {
                        await ImportLoRaDevice(csvReader, deviceId, deviceName, modelId, deviceTags);
                    }
                    else
                    {
                        await ImportDevice(csvReader, deviceId, deviceName, modelId, deviceTags);
                    }
                }
                catch (Exception e)
                {
                    report.Add(new ImportResultLine
                    {
                        DeviceId = deviceId,
                        LineNumber = lineNumber,
                        IsErrorMessage = true,
                        Message = e.Message

                    });

                    continue;
                }
            }

            return report;
        }

        private static Dictionary<string, string> ReadTags(CsvReader reader, IEnumerable<string> tagsToRead)
        {
            var deviceTags = new Dictionary<string,string>();

            foreach (var tag in tagsToRead)
            {
                if (reader.TryGetField<string>($"{TagPrefix}:{tag}", out var tagValue))
                    deviceTags.Add(tag, tagValue);
            }

            return deviceTags;
        }

        private static bool TryReadMandatoryFields(CsvReader reader, int lineNumber, ref string deviceId, ref string deviceName, ref string modelId, ref List<ImportResultLine> report)
        {
            if (!reader.TryGetField<string>("Id", out deviceId) || deviceId.IsNullOrEmpty())
            {
                report.Add(new ImportResultLine
                {
                    DeviceId = "-1",
                    LineNumber = lineNumber,
                    IsErrorMessage = true,
                    Message = "The parameter Id cannot be null or empty"

                });

                return false;
            }

            if (!reader.TryGetField<string>("Name", out deviceName) || deviceName.IsNullOrEmpty())
            {
                report.Add(new ImportResultLine
                {
                    DeviceId = deviceId,
                    LineNumber = lineNumber,
                    IsErrorMessage = true,
                    Message = "The parameter Name cannot be null or empty"

                });

                return false;
            }

            if (!reader.TryGetField<string>("ModelId", out modelId) || modelId.IsNullOrEmpty())
            {
                report.Add(new ImportResultLine
                {
                    DeviceId = deviceId,
                    LineNumber = lineNumber,
                    IsErrorMessage = true,
                    Message = "The parameter ModelId cannot be null or empty"

                });

                return false;
            }

            return true;
        }

        private async Task ImportLoRaDevice(
            CsvReader csvReader,
            string deviceId, string deviceName, string modelId,
            Dictionary<string, string> deviceTags)
        {
            var newDevice = new LoRaDeviceDetails()
            {
                DeviceID = deviceId,
                DeviceName = deviceName,
                ModelId = modelId,
                Tags = deviceTags,
                IsEnabled = true
            };

            TryReadProperty(csvReader, newDevice, c => c.AppKey, string.Empty);
            TryReadProperty(csvReader, newDevice, c => c.AppEUI, string.Empty);
            TryReadProperty(csvReader, newDevice, c => c.AppSKey, string.Empty);
            TryReadProperty(csvReader, newDevice, c => c.NwkSKey, string.Empty);
            TryReadProperty(csvReader, newDevice, c => c.DevAddr, string.Empty);
            TryReadProperty(csvReader, newDevice, c => c.GatewayID, string.Empty);
            TryReadProperty(csvReader, newDevice, c => c.Downlink, null);
            TryReadProperty(csvReader, newDevice, c => c.ClassType, ClassType.A);
            TryReadProperty(csvReader, newDevice, c => c.PreferredWindow, 1);
            TryReadProperty(csvReader, newDevice, c => c.Deduplication, DeduplicationMode.Drop);
            TryReadProperty(csvReader, newDevice, c => c.RX1DROffset, null);
            TryReadProperty(csvReader, newDevice, c => c.RX2DataRate, null);
            TryReadProperty(csvReader, newDevice, c => c.RXDelay, null);
            TryReadProperty(csvReader, newDevice, c => c.ABPRelaxMode, null);
            TryReadProperty(csvReader, newDevice, c => c.SensorDecoder, string.Empty);
            TryReadProperty(csvReader, newDevice, c => c.FCntUpStart, null);
            TryReadProperty(csvReader, newDevice, c => c.FCntDownStart, null);
            TryReadProperty(csvReader, newDevice, c => c.FCntResetCounter, null);
            TryReadProperty(csvReader, newDevice, c => c.Supports32BitFCnt, null);
            TryReadProperty(csvReader, newDevice, c => c.KeepAliveTimeout, null);

            _ = await this.loraDeviceService.CheckIfDeviceExists(newDevice.DeviceID)
                ? await this.loraDeviceService.UpdateDevice(newDevice)
                : await this.loraDeviceService.CreateDevice(newDevice);
        }

        private async Task ImportDevice(CsvReader csvReader,
            string deviceId, string deviceName, string modelId,
            Dictionary<string, string> deviceTags)
        {
            var deviceProperties = new List<DevicePropertyValue>();
            var properties = await this.deviceModelPropertiesService.GetModelProperties(modelId);

            foreach (var property in properties)
            {
                deviceProperties.Add(new DevicePropertyValue()
                {
                    Name = property.Name,
                    PropertyType = property.PropertyType,
                    Value = csvReader.GetField($"{PropertyPrefix}:{property.Name}")
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

        private static void TryReadProperty<T, TValue>(CsvReader reader, T device, Expression<Func<T, TValue>> expression, TValue defaultValue = default)
        {
            var memberExpression = expression.Body as MemberExpression;

            var result = reader.TryGetField<TValue>($"{PropertyPrefix}:{memberExpression.Member.Name}", out var property) ? property : defaultValue;

            (memberExpression.Member as PropertyInfo).SetValue(device, result);
        }
    }
}

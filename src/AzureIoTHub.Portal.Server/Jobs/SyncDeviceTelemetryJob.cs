// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Jobs
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Azure.Messaging.EventHubs.Consumer;
    using Domain;
    using Domain.Entities;
    using Domain.Repositories;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using System.Globalization;

    public class SyncDeviceTelemetryJob : IJob
    {
        private readonly ILogger<SyncDeviceTelemetryJob> logger;
        private readonly ConfigHandler configHandler;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly IDeviceTelemetryRepository deviceTelemetryRepository;
        private readonly IUnitOfWork unitOfWork;

        public SyncDeviceTelemetryJob(ILogger<SyncDeviceTelemetryJob> logger,
            ConfigHandler configHandler,
            ILorawanDeviceRepository lorawanDeviceRepository,
            IDeviceTelemetryRepository deviceTelemetryRepository,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.configHandler = configHandler;
            this.lorawanDeviceRepository = lorawanDeviceRepository;
            this.deviceTelemetryRepository = deviceTelemetryRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using var consumer = new EventHubConsumerClient(this.configHandler.IoTHubEventHubConsumerGroup, this.configHandler.IoTHubEventHubEndpoint);

            await foreach (var messageEvent in consumer.ReadEventsAsync(new ReadEventOptions { MaximumWaitTime = TimeSpan.FromMilliseconds(500) }, context.CancellationToken))
            {
                if (messageEvent.Data == null) continue;

                DeviceTelemetry deviceTelemetry;

                try
                {
                    deviceTelemetry = new DeviceTelemetry
                    {
                        Id = messageEvent.Data.SequenceNumber.ToString(CultureInfo.InvariantCulture),
                        EnqueuedTime = messageEvent.Data.EnqueuedTime.UtcDateTime,
                        Telemetry = messageEvent.Data.EventBody.ToObjectFromJson<Telemetry>()
                    };
                }
                catch (JsonException)
                {
                    this.logger.LogWarning($"Unable to deserialize the event message with id {messageEvent.Data.SequenceNumber} as device telemetry");
                    continue;
                }

                var loRaWanDevice = await this.lorawanDeviceRepository.GetByIdAsync(deviceTelemetry.Telemetry.DeviceEUI, device => device.Telemetries);

                if (loRaWanDevice == null)
                {
                    continue;
                }

                if (loRaWanDevice.Telemetries.Any(telemetry => telemetry.Id.Equals(deviceTelemetry.Id, StringComparison.Ordinal)))
                {
                    continue;
                }

                loRaWanDevice.Telemetries.Add(deviceTelemetry);

                await this.unitOfWork.SaveAsync();

                await KeepOnlyLatestHundredTelemetries(loRaWanDevice);
            }
        }

        private async Task KeepOnlyLatestHundredTelemetries(LorawanDevice loRaWanDevice)
        {
            if (loRaWanDevice.Telemetries.Count > 100) return;

            loRaWanDevice.Telemetries.OrderByDescending(telemetry => telemetry.EnqueuedTime)
                    .Skip(100)
                    .ToList()
                    .ForEach(telemetry =>
                    {
                        this.deviceTelemetryRepository.Delete(telemetry.Id);
                    });

            await this.unitOfWork.SaveAsync();
        }
    }
}

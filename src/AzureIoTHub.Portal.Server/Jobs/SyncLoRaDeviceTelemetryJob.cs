// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Jobs
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Entities;
    using Domain.Repositories;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using System.Globalization;
    using Azure.Messaging.EventHubs;
    using Azure.Storage.Blobs;
    using System.Threading;
    using Azure.Messaging.EventHubs.Processor;
    using Microsoft.EntityFrameworkCore;

    public class SyncLoRaDeviceTelemetryJob : IJob
    {
        private readonly ILogger<SyncLoRaDeviceTelemetryJob> logger;
        private readonly ConfigHandler configHandler;
        private readonly BlobServiceClient blobServiceClient;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly ILoRaDeviceTelemetryRepository deviceTelemetryRepository;
        private readonly IUnitOfWork unitOfWork;

        public SyncLoRaDeviceTelemetryJob(ILogger<SyncLoRaDeviceTelemetryJob> logger,
            ConfigHandler configHandler,
            BlobServiceClient blobServiceClient,
            ILorawanDeviceRepository lorawanDeviceRepository,
            ILoRaDeviceTelemetryRepository deviceTelemetryRepository,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.configHandler = configHandler;
            this.blobServiceClient = blobServiceClient;
            this.lorawanDeviceRepository = lorawanDeviceRepository;
            this.deviceTelemetryRepository = deviceTelemetryRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var storageClient = this.blobServiceClient.GetBlobContainerClient("iothub-portal-events-checkpoints");

            _ = await storageClient.CreateIfNotExistsAsync();

            var processor = new EventProcessorClient(
                storageClient,
                this.configHandler.IoTHubEventHubConsumerGroup,
                this.configHandler.IoTHubEventHubEndpoint);

            try
            {
                processor.ProcessEventAsync += ProcessEventHandler;
                processor.ProcessErrorAsync += ProcessErrorHandler;

                try
                {
                    await processor.StartProcessingAsync(context.CancellationToken);
                    await Task.Delay(Timeout.Infinite, context.CancellationToken);
                }
                finally
                {
                    await processor.StopProcessingAsync();
                }
            }
            finally
            {
                processor.ProcessEventAsync -= ProcessEventHandler;
                processor.ProcessErrorAsync -= ProcessErrorHandler;
            }
        }

        private async Task ProcessEventHandler(ProcessEventArgs args)
        {
            try
            {
                if (args.Data == null) return;

                LoRaDeviceTelemetry deviceTelemetry;

                try
                {
                    deviceTelemetry = new LoRaDeviceTelemetry
                    {
                        Id = args.Data.SequenceNumber.ToString(CultureInfo.InvariantCulture),
                        EnqueuedTime = args.Data.EnqueuedTime.UtcDateTime,
                        Telemetry = args.Data.EventBody.ToObjectFromJson<LoRaTelemetry>()
                    };
                }
                catch (JsonException)
                {
                    this.logger.LogWarning($"Unable to deserialize the event message with id {args.Data.SequenceNumber} as device telemetry");
                    return;
                }

                var loRaWanDevice = await this.lorawanDeviceRepository.GetByIdAsync(deviceTelemetry.Telemetry.DeviceEUI, device => device.Telemetry);

                if (loRaWanDevice == null)
                {
                    return;
                }

                if (loRaWanDevice.Telemetry.Any(telemetry => telemetry.Id.Equals(deviceTelemetry.Id, StringComparison.Ordinal)))
                {
                    return;
                }

                loRaWanDevice.Telemetry.Add(deviceTelemetry);

                await this.unitOfWork.SaveAsync();

                await KeepOnlyLatestHundredTelemetries(loRaWanDevice);
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, $"Unable to store the LoRa telemetry message with sequence number {args.Data.SequenceNumber}");
            }

            return;
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs args)
        {
            this.logger.LogError(args.Exception, $"Error in the EventProcessorClient on {nameof(SyncLoRaDeviceTelemetryJob)}: Operation {args.Operation}");

            return Task.CompletedTask;
        }

        private async Task KeepOnlyLatestHundredTelemetries(LorawanDevice loRaWanDevice)
        {
            if (loRaWanDevice.Telemetry.Count <= 100) return;

            loRaWanDevice.Telemetry
                .OrderByDescending(telemetry => telemetry.EnqueuedTime)
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

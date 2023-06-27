// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Jobs
{
    using Domain;
    using global::Azure.Messaging.EventHubs;
    using global::Azure.Messaging.EventHubs.Processor;
    using global::Azure.Storage.Blobs;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using System.Threading;
    using System.Threading.Tasks;

    public class SyncLoRaDeviceTelemetryJob : IJob
    {
        private readonly ILogger<SyncLoRaDeviceTelemetryJob> logger;
        private readonly ConfigHandler configHandler;
        private readonly BlobServiceClient blobServiceClient;
        private readonly IDeviceService<LoRaDeviceDetailsDto> deviceService;

        private const string CHECKPOINTS_BLOBSTORAGE_NAME = "iothub-portal-events-checkpoints";

        public SyncLoRaDeviceTelemetryJob(
            ILogger<SyncLoRaDeviceTelemetryJob> logger,
            ConfigHandler configHandler,
            BlobServiceClient blobServiceClient,
            IDeviceService<LoRaDeviceDetailsDto> deviceService)
        {
            this.logger = logger;
            this.configHandler = configHandler;
            this.blobServiceClient = blobServiceClient;
            this.deviceService = deviceService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var storageClient = this.blobServiceClient.GetBlobContainerClient(CHECKPOINTS_BLOBSTORAGE_NAME);

            _ = await storageClient.CreateIfNotExistsAsync();

            var processor = new EventProcessorClient(
                storageClient,
                this.configHandler.AzureIoTHubEventHubConsumerGroup,
                this.configHandler.AzureIoTHubEventHubEndpoint);

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
            await this.deviceService.ProcessTelemetryEvent(args.Data);
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs args)
        {
            this.logger.LogError(args.Exception, $"Error in the EventProcessorClient on {nameof(SyncLoRaDeviceTelemetryJob)}: Operation {args.Operation}");

            return Task.CompletedTask;
        }
    }
}

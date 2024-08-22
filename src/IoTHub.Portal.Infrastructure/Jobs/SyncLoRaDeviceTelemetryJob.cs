// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Messaging.EventHubs;
    using Azure.Messaging.EventHubs.Processor;
    using Azure.Storage.Blobs;
    using IoTHub.Portal.Application.Services;
    using Domain;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Shared.Models.v1._0.LoRaWAN;

    public class SyncLoRaDeviceTelemetryJob : IJob
    {
        private readonly ILogger<SyncLoRaDeviceTelemetryJob> logger;
        private readonly ConfigHandler configHandler;
        private readonly BlobServiceClient blobServiceClient;
        private readonly IDeviceService<LoRaDeviceDetails> deviceService;

        private const string CHECKPOINTS_BLOBSTORAGE_NAME = "iothub-portal-events-checkpoints";

        public SyncLoRaDeviceTelemetryJob(
            ILogger<SyncLoRaDeviceTelemetryJob> logger,
            ConfigHandler configHandler,
            BlobServiceClient blobServiceClient,
            IDeviceService<LoRaDeviceDetails> deviceService)
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

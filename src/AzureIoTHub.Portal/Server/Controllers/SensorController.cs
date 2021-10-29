// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using Azure.Storage.Blobs;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    public class SensorController : ControllerBase
    {
        private readonly ILogger<SensorController> logger;
        private readonly TableClient tableClient;
        private readonly IConfiguration configuration;
        private readonly BlobServiceClient blobService;

        public SensorController(
            ILogger<SensorController> logger,
            IConfiguration configuration,
            BlobServiceClient blobServiceClient,
            TableClient tableClient)
        {
            this.logger = logger;
            this.tableClient = tableClient;
            this.configuration = configuration;
            this.blobService = blobServiceClient;
        }

        [HttpPost]
        // public IActionResult Post(SensorModel sensor)
        public async Task Post([FromForm]SensorModel sensor, [FromForm] IFormFile file = null)
        {
            try
            {
                var test = sensor;
                TableEntity entity = new TableEntity();
                entity.PartitionKey = "0";
                entity.RowKey = sensor.Name;

                entity["Description"] = sensor.Description;
                entity["AppEUI"] = sensor.AppEUI;
                if (file != null)
                {
                    entity["Image"] = file.Name;
                    BlobContainerClient blobContainer = this.blobService.GetBlobContainerClient(this.configuration["StorageAcount:BlobContainerName"]);
                    BlobClient blobClient = blobContainer.GetBlobClient(file.Name);
                    this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");
                    await blobClient.UploadAsync(file.OpenReadStream());
                }

                // this.tableClient.AddEntity(entity);
                // insertion des commant
                // if (sensor.Commands.Count > 0)
                // {
                //    foreach (var element in sensor.Commands)
                //    {
                //        TableEntity commandEntity = new TableEntity();
                //        commandEntity.PartitionKey = sensor.Name;
                //        commandEntity.RowKey = element.Name;
                //        commandEntity["Trame"] = element.Trame;
                //        commandEntity["Port"] = element.Port;
                //        this.tableClient.AddEntity(commandEntity);
                //    }
                // }

                // return this.Ok("tout va bien");
            }
            catch (Exception)
            {
                // return this.BadRequest(e.Message);
            }
        }
    }
}

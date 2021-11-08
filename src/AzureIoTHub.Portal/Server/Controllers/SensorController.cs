// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using Newtonsoft.Json;

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
        public async Task<IActionResult> Post([FromForm]string sensor, [FromForm] IFormFile file = null)
        {
            try
            {
                SensorModel sensorObject = JsonConvert.DeserializeObject<SensorModel>(sensor);
                TableEntity entity = new TableEntity();
                entity.PartitionKey = "0";
                entity.RowKey = sensorObject.Name;

                entity["Description"] = sensorObject.Description;
                entity["AppEUI"] = sensorObject.AppEUI;

                if (file != null)
                {
                    entity["Image"] = file.FileName;
                    BlobContainerClient blobContainer = this.blobService.GetBlobContainerClient(this.configuration["StorageAcount:BlobContainerName"]);
                    BlobClient blobClient = blobContainer.GetBlobClient(file.FileName);

                    this.logger.LogInformation($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");

                    await blobClient.UploadAsync(file.OpenReadStream());
                }

                this.tableClient.AddEntity(entity);
                // insertion des commant
                if (sensorObject.Commands.Count > 0)
                {
                    foreach (var element in sensorObject.Commands)
                    {
                        TableEntity commandEntity = new TableEntity();
                        commandEntity.PartitionKey = sensorObject.Name;
                        commandEntity.RowKey = element.Name;
                        commandEntity["Trame"] = element.Trame;
                        commandEntity["Port"] = element.Port;
                        this.tableClient.AddEntity(commandEntity);
                    }
                }

                return this.Ok("tout va bien");
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }
    }
}

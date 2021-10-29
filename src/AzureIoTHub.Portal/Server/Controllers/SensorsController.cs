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
    public class SensorsController : ControllerBase
    {
        private readonly ILogger<SensorsController> logger;
        private readonly TableClient tableClient;
        private readonly IConfiguration configuration;
        private readonly BlobServiceClient blobService;

        public SensorsController(
            ILogger<SensorsController> logger,
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

        /// <summary>
        /// Gets a list of sensor models from an Azure DataTable.
        /// </summary>
        /// <returns>A list of SensorModel.</returns>
        [HttpGet]
        public async Task<IEnumerable<SensorModel>> Get()
        {
            await Task.Delay(0);

            // PartitionKey 0 contains all sensor models
            Pageable<TableEntity> entities = this.tableClient.Query<TableEntity>("PartitionKey eq '0'");

            // Converts the query result into a list of sensor models
            IEnumerable<SensorModel> sensorsList = entities.Select(e => this.MapTableEntityToSensorModel(e));
            return sensorsList;
        }

        /// <summary>
        /// Creates a SensorModel object from a query result.
        /// Checks first if the entity fields fit to the sensor model attributes.
        /// </summary>
        /// <param name="entity">An AzureDataTable entity coming from a query.</param>
        /// <returns>A sensor model.</returns>
        public SensorModel MapTableEntityToSensorModel(TableEntity entity)
        {
            SensorModel sensor = new SensorModel();
            sensor.Name = entity.RowKey;
            if (entity.ContainsKey("Description"))
            {
                sensor.Description = entity["Description"].ToString();
            }

            if (entity.ContainsKey("AppEUI"))
            {
                sensor.AppEUI = entity["AppEUI"].ToString();
            }

            return sensor;
        }
    }
}

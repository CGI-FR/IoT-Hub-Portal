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
    public class BlobContainersController : ControllerBase
    {
        private readonly ILogger<SensorsController> logger;
        private readonly TableClient tableClient;
        private readonly IConfiguration configuration;
        private readonly BlobServiceClient blobService;

        public BlobContainersController(
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

        /// <summary>
        /// Retrieves the blob container URL where to fetch the image from.
        /// </summary>
        /// <returns>The URL of the blob containers containing sensor models' pictures.</returns>
        [HttpGet("{sensorName}")]
        public async Task<string> Get(string sensorName)
        {
            string imgUrl = "https://"
                + this.configuration["StorageAcount:AccountName"]
                + ".blob.core.windows.net/"
                + this.configuration["StorageAcount:BlobContainerName"]
                + "/"
                + sensorName;

            // Checks whether the picture exists
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(imgUrl);
            if (!response.IsSuccessStatusCode)
            {
                imgUrl = "images/error.png";
            }

            return imgUrl;
        }
    }
}
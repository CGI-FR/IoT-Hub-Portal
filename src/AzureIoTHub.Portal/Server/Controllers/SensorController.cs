// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
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

        public SensorController(ILogger<SensorController> logger, TableClient tableClient)
        {
            this.logger = logger;
            this.tableClient = tableClient;
        }

        [HttpPost]
        public void Post(SensorModel sensor)
        {
            TableEntity entity = new TableEntity();
            entity.PartitionKey = sensor.Name;
            entity.RowKey = sensor.AppEUI;

            entity["Description"] = sensor.Description;
            this.tableClient.AddEntity(entity);
            // return this.Ok();
        }
    }
}

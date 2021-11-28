// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    public class BlobContainersController : ControllerBase
    {
        private readonly ISensorImageManager sensorImageManager;

        public BlobContainersController(ISensorImageManager sensorImageManager)
        {
            this.sensorImageManager = sensorImageManager;
        }

        /// <summary>
        /// Retrieves the blob container URL where to fetch the image from.
        /// </summary>
        /// <returns>The URL of the blob containers containing sensor models' pictures.</returns>
        [HttpGet("{sensorName}")]
        public async Task<string> Get(string sensorName)
        {
            var result = await this.sensorImageManager.GetSensorImageUriAsync(sensorName);

            if (result == null)
            {
                return "images/error.png";
            }

            return result.ToString();
        }
    }
}
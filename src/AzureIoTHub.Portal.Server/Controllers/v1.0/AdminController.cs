// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.IO;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Managers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/admin")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> logger;
        private readonly IExportManager exportManager;

        public AdminController(
            ILogger<AdminController> logger,
            IExportManager exportManager)
        {
            this.exportManager = exportManager;
            this.logger = logger;
        }

        [HttpGet("export/devices", Name = "Export devices")]
        public async Task<Stream> ExportDeviceList()
        {
            var stream = await this.exportManager.ExportDeviceList();
            return stream;

            //var httpResponseMessage = new HttpResponseMessage
            //{
            //    Content = new StreamContent(stream)
            //};
            //httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            //{
            //    FileName = "test2.csv"
            //};
            //httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            //return httpResponseMessage;
        }
    }
}

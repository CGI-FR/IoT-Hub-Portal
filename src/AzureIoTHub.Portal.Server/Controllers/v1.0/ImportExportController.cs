// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.IO;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Services;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class ImportExportController : ControllerBase
    {
        private readonly ILogger<ImportExportController> logger;
        private readonly IDeviceService<DeviceDetails> deviceService;
        private readonly IDeviceService<LoRaDeviceDetails> lorawanDeviceService;

        public ImportExportController(
            ILogger<ImportExportController> logger,
            IDeviceService<LoRaDeviceDetails> lorawanDeviceService,
            IDeviceService<DeviceDetails> deviceService)
        {
            this.lorawanDeviceService = lorawanDeviceService;
            this.deviceService = deviceService;
            this.logger = logger;
        }

        [HttpGet("export/devices", Name = "Export devices")]
        public async Task<Stream> ExportDeviceList()
        {
            var stream = await this.deviceService.ExportDeviceList();
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

        [HttpGet("export/lorawandevices", Name = "Export lorawan devices")]
        public async Task<Stream> ExportLorawanDeviceList()
        {
            return await this.lorawanDeviceService.ExportDeviceList();
        }
    }
}

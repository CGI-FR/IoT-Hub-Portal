// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Managers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models.v1._0;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/admin")]
    [ApiExplorerSettings(GroupName = "Admin APIs")]
    public class AdminController : ControllerBase
    {
        private readonly IExportManager exportManager;

        public AdminController(IExportManager exportManager)
        {
            this.exportManager = exportManager;
        }

        [HttpPost("devices/_export", Name = "Export devices")]
        public async Task<IActionResult> ExportDeviceList()
        {
            var stream = new MemoryStream();

            await this.exportManager.ExportDeviceList(stream);
            stream.Position = 0;

            return this.File(stream, "text/csv", $"Devices_{DateTime.Now:yyyyMMddHHmm}.csv");
        }

        [HttpPost("devices/_template", Name = "Download template file")]
        public async Task<IActionResult> ExportTemplateFile()
        {
            var stream = new MemoryStream();

            await this.exportManager.ExportTemplateFile(stream);
            stream.Position = 0;

            return this.File(stream, "text/csv", $"Devices_Template.csv");
        }

        [HttpPost("devices/_import", Name = "Import devices")]
        public async Task<ActionResult<ImportResultLine[]>> ImportDeviceList(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var errorReport = await this.exportManager.ImportDeviceList(stream);


            return Ok(errorReport);
        }
    }
}

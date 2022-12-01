// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v1._0
{
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/edge/models/{id}/commands")]
    public class EdgeModuleCommandsController : Controller
    {

        private readonly IEdgeModuleCommandsService edgeModuleCommandsService;

        public EdgeModuleCommandsController(IEdgeModuleCommandsService edgeModuleCommandsService)
        {
            this.edgeModuleCommandsService = edgeModuleCommandsService;
        }

        //[HttpPost]
        //public async Task<IActionResult> PostEdgeModuleCommand(string id, List<EdgeModuleCommandDto> edgeModuleCommands)
        //{
        //    ArgumentNullException.ThrowIfNull(id, nameof(id));
        //    ArgumentNullException.ThrowIfNull(edgeModuleCommands, nameof(edgeModuleCommands));

        //    await this.edgeModuleCommandsService.SaveEdgeModuleCommandAsync(id, edgeModuleCommands);
        //    return Ok();
        //}

        //[HttpGet]
        //public async Task<IEnumerable<EdgeModuleCommandDto>> GetEdgeModuleCommandList(string modelId)
        //{
        //    ArgumentNullException.ThrowIfNull(modelId, nameof(modelId));

        //    return await this.edgeModuleCommandsService.GetAllEdgeModule(modelId);
        //}
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Exceptions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.Filters;

    [Authorize]
    [Route("api/edge/models")]
    [ApiExplorerSettings(GroupName = "IoT Edge Devices Models")]
    [ApiController]
    public class EdgeModelsController : ControllerBase
    {
        private readonly IEdgeModelService edgeModelService;

        public EdgeModelsController(
            IEdgeModelService edgeModelService)
        {
            this.edgeModelService = edgeModelService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IoTEdgeModelListItem>>> GetEdgeModelList([FromQuery] EdgeModelFilter edgeModelFilter)
        {
            return Ok(await this.edgeModelService.GetEdgeModels(edgeModelFilter));
        }

        [HttpGet("{edgeModelId}")]
        public async Task<ActionResult<IoTEdgeModel>> GetEdgeDeviceModel(string edgeModelId)
        {
            return Ok(await this.edgeModelService.GetEdgeModel(edgeModelId));
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdgeModel(IoTEdgeModel EdgeModel)
        {
            await this.edgeModelService.CreateEdgeModel(EdgeModel);

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEdgeModel(IoTEdgeModel EdgeModel)
        {
            await this.edgeModelService.UpdateEdgeModel(EdgeModel);

            return Ok();
        }

        /// <summary>
        /// Delete the edge device model.
        /// </summary>
        /// <param name="edgeModelId">the model id.</param>
        /// <returns>Http response</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpDelete("{edgeModelId}")]
        public async Task<IActionResult> DeleteModelAsync(string edgeModelId)
        {
            await this.edgeModelService.DeleteEdgeModel(edgeModelId);

            return NoContent();
        }

        /// <summary>
        /// Gets the avatar.
        /// </summary>
        /// <param name="edgeModelId">The model identifier.</param>
        /// <returns>The avatar.</returns>
        [HttpGet("{edgeModelId}/avatar", Name = "GET edge Device model avatar URL")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<string>> GetAvatar(string edgeModelId)
        {
            return Ok(await this.edgeModelService.GetEdgeModelAvatar(edgeModelId));
        }

        /// <summary>
        /// Changes the avatar.
        /// </summary>
        /// <param name="edgeModelId">The model identifier.</param>
        /// <param name="file">The file.</param>
        /// <returns>The avatar.</returns>
        [HttpPost("{edgeModelId}/avatar", Name = "POST Update the edge device model avatar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<string>> ChangeAvatar(string edgeModelId, IFormFile file)
        {
            return Ok(await this.edgeModelService.UpdateEdgeModelAvatar(edgeModelId, file));
        }

        /// <summary>
        /// Deletes the avatar.
        /// </summary>
        /// <param name="edgeModelId">The model identifier.</param>
        [HttpDelete("{edgeModelId}/avatar", Name = "DELETE Remove the edge device model avatar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> DeleteAvatar(string edgeModelId)
        {
            await this.edgeModelService.DeleteEdgeModelAvatar(edgeModelId);
            return NoContent();
        }

        /// <summary>
        /// Get public edge modules
        /// </summary>
        /// <returns>Public edge modules</returns>
        [HttpGet("public-modules", Name = "GET edge public modules")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<IEnumerable<IoTEdgeModel>>> GetPublicEdgeModules()
        {
            return Ok(await this.edgeModelService.GetPublicEdgeModules());
        }
    }
}

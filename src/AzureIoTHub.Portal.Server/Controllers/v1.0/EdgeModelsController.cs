// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v10
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

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
        public ActionResult<IEnumerable<IoTEdgeModelListItem>> GetEdgeModelList()
        {
            return Ok(this.edgeModelService.GetEdgeModels());
        }

        [HttpGet("{edgeModelId}")]
        public async Task<ActionResult<IoTEdgeModel>> GetEdgeDeviceModel(string edgeModelId)
        {
            return Ok(await this.edgeModelService.GetEdgeModel(edgeModelId));
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdgeModel(IoTEdgeModel EdgeModel)
        {
            var test = new IoTEdgeModel
            {
                ModelId = "1234",
                Name = "testModel01",
                Description = "model de test pour les nouvelles command.",
                EdgeModules = new List<IoTEdgeModule>()
                {
                    new IoTEdgeModule
                    {
                        ModuleName = "moduleTest",
                        ImageURI = "imageUriTest",
                        Commands = new List<EdgeModuleCommandDto>
                        {
                            new EdgeModuleCommandDto
                            {
                                Id = "commandId",
                                Type = "Command",
                                Name = "getMaxMinReport",
                                DisplayName = "Get Max-Min report.",
                                Description = "This command returns the max, min and average temperature from the specified time to the current time.",
                                Request = new EdgeModuleCommandPayloadDto
                                {
                                    Name = "since",
                                    DisplayName = "since",
                                    Description = "Period to return the max-min report.",
                                    Schema = "dateTime"
                                },
                                Response = new EdgeModuleCommandPayloadDto
                                {
                                    Name = "tempReport",
                                    DisplayName = "Temperature Report",
                                    Schema = new DigitalTwinObjectTypeDto
                                    {
                                        Fileds = new List<DigitalTwinFieldTypeDto>
                                        {
                                            new DigitalTwinFieldTypeDto
                                            {
                                                Name = "maxTemp",
                                                DisplayName = "Max temperature",
                                                Schema = "double"
                                            },
                                            new DigitalTwinFieldTypeDto
                                            {
                                                Name = "minTemp",
                                                DisplayName = "Min temperature",
                                                Schema = "double"
                                            },
                                            new DigitalTwinFieldTypeDto
                                            {
                                                Name = "avgTemp",
                                                DisplayName = "Average temperature",
                                                Schema = "double"
                                            },
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await this.edgeModelService.CreateEdgeModel(test);
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
    }
}

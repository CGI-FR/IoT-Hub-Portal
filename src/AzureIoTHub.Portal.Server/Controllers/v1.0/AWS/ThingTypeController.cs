// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v1._0.AWS
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/aws/thingtypes")]
    [ApiExplorerSettings(GroupName = "IoT Thing Types")]
    public class ThingTypeController : ControllerBase
    {
        private readonly IThingTypeService thingTypeService;

        public ThingTypeController(IThingTypeService thingTypeService)
        {
            this.thingTypeService = thingTypeService;
        }

        /// <summary>
        /// Creates the Thing type.
        /// </summary>
        /// <param name="thingtype">The thing type.</param>
        [HttpPost(Name = "POST Create AWS Thing type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> CreateThingTypeAsync(ThingTypeDto thingtype)
        {

            return Ok(await this.thingTypeService.CreateThingType(thingtype));
        }

        /// <summary>
        /// Gets the thing type avatar.
        /// </summary>
        /// <param name="id">The thing type identifier</param>
        [HttpGet("{id}/avatar", Name = "GET thing typel avatar URL")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GetAvatar(string id)
        {
            return Ok(await this.thingTypeService.GetThingTypeAvatar(id));
        }

        /// <summary>
        /// Changes the avatar.
        /// </summary>
        /// <param name="id">The thing type identifier.</param>
        /// <param name="file">The file.</param>
        /// <returns>The avatar.</returns>
        [HttpPost("{id}/avatar", Name = "POST Update the thing type avatar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> ChangeAvatar(string id, IFormFile file)
        {
            return Ok(await this.thingTypeService.UpdateThingTypeAvatar(id, file));
        }

        /// <summary>
        /// Deletes the avatar.
        /// </summary>
        /// <param name="id">The thing type identifier.</param>
        [HttpDelete("{id}/avatar", Name = "DELETE Remove the thing type avatar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAvatar(string id)
        {
            await this.thingTypeService.DeleteThingTypeAvatar(id);
            return NoContent();
        }
    }
}

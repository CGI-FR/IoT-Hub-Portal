// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v1._0.AWS
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;

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
        /// Gets the Thing type list.
        /// </summary>
        /// <returns>An array representing the Thing type.</returns>
        [HttpGet(Name = "GET Thing type list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<ThingTypeDto>>> GetThingTypes([FromQuery] DeviceModelFilter deviceModelFilter)
        {
            var paginatedThingType = await this.thingTypeService.GetThingTypes(deviceModelFilter);

            var nextPage = string.Empty;

            if (paginatedThingType.HasNextPage)
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    RouteName = "GET Thing Type list",
                    Values = new
                    {
                        deviceModelFilter.SearchText,
                        deviceModelFilter.PageSize,
                        pageNumber = deviceModelFilter.PageNumber + 1,
                        deviceModelFilter.OrderBy
                    }
                });
            }

            return Ok(new PaginationResult<ThingTypeDto>
            {
                Items = paginatedThingType.Data,
                TotalItems = paginatedThingType.TotalCount,
                NextPage = nextPage
            });
        }

        /// <summary>
        /// Gets a thing type.
        /// </summary>
        /// <returns>An array representing the Thing type.</returns>
        [HttpGet("{id}", Name = "GET A Thing type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ThingTypeDto>> GetThingType(string id)
        {
            return Ok(await thingTypeService.GetThingType(id));
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
        /// Deprecate the Thing type.
        /// </summary>
        /// <param name="id">The thing type.</param>
        [HttpPut("{id}", Name = "PUT Create AWS Thing type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ThingTypeDto>> DeprecateThingTypeAsync(string id)
        {
            return Ok(await this.thingTypeService.DeprecateThingType(id));
        }

        /// <summary>
        /// Deletes the thing type.
        /// </summary>
        /// <param name="id">The thing type identifier.</param>
        [HttpDelete("{id}", Name = "DELETE the thing type")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteThingTypeAsync(string id)
        {
            await this.thingTypeService.DeleteThingType(id);
            return NoContent();
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
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
        public async Task<IActionResult> CreateThingTypeAsync(ThingTypeDto thingtype)
        {
            _ = await this.thingTypeService.CreateThingType(thingtype);

            return Ok(thingtype);
        }
    }
}

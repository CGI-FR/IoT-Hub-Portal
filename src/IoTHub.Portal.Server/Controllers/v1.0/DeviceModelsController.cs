// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.Filters;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/models")]
    [ApiExplorerSettings(GroupName = "Device Models")]
    public class DeviceModelsController : DeviceModelsControllerBase<DeviceModelDto, DeviceModelDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModelsController"/> class.
        /// </summary>
        /// <param name="deviceModelService">Device Model Service</param>
        public DeviceModelsController(IDeviceModelService<DeviceModelDto, DeviceModelDto> deviceModelService)
            : base(deviceModelService)
        {
        }

        /// <summary>
        /// Gets the device model list.
        /// </summary>
        /// <returns>An array representing the device models.</returns>
        [HttpGet(Name = "GET Device model list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public override async Task<ActionResult<PaginationResult<DeviceModelDto>>> GetItems([FromQuery] DeviceModelFilter deviceModelFilter)
        {
            return await base.GetItems(deviceModelFilter);
        }

        /// <summary>
        /// Get the device model details.
        /// </summary>
        /// <param name="id">The devic emodel identifier.</param>
        /// <returns>The device model details.</returns>
        [HttpGet("{id}", Name = "GET Device model")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<ActionResult<DeviceModelDto>> GetItem(string id)
        {
            return base.GetItem(id);
        }

        /// <summary>
        /// Gets the device model avatar.
        /// </summary>
        /// <param name="id">The device model identifier</param>
        [HttpGet("{id}/avatar", Name = "GET Device model avatar URL")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<ActionResult<string>> GetAvatar(string id)
        {
            return base.GetAvatar(id);
        }

        /// <summary>
        /// Changes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="file">The file.</param>
        /// <returns>The avatar.</returns>
        [HttpPost("{id}/avatar", Name = "POST Update the device model avatar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<ActionResult<string>> ChangeAvatar(string id, IFormFile file)
        {
            return base.ChangeAvatar(id, file);
        }

        /// <summary>
        /// Deletes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        [HttpDelete("{id}/avatar", Name = "DELETE Remove the device model avatar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<IActionResult> DeleteAvatar(string id)
        {
            return base.DeleteAvatar(id);
        }

        /// <summary>
        /// Creates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        [HttpPost(Name = "POST Create a new device model")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public override Task<IActionResult> Post(DeviceModelDto deviceModel)
        {
            return base.Post(deviceModel);
        }

        /// <summary>
        /// Updates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        [HttpPut(Name = "PUT Update the device model")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<IActionResult> Put(DeviceModelDto deviceModel)
        {
            return base.Put(deviceModel);
        }

        /// <summary>
        /// Deletes the specified device model.
        /// </summary>
        /// <param name="id">The device model identifier.</param>
        /// <returns>The action result.</returns>
        [HttpDelete("{id}", Name = "DELETE Remove the device model")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<IActionResult> Delete(string id)
        {
            return base.Delete(id);
        }
    }
}

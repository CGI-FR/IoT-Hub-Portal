// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0.LoRaWAN
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using IoTHub.Portal.Server.Filters;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/models")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANDeviceModelsController : DeviceModelsControllerBase<DeviceModelDto, LoRaDeviceModelDto>
    {
        private readonly IDeviceModelService<DeviceModelDto, LoRaDeviceModelDto> deviceModelService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaWANDeviceModelsController"/> class.
        /// </summary>
        /// <param name="deviceModelService">Device Model Service</param>
        public LoRaWANDeviceModelsController(IDeviceModelService<DeviceModelDto, LoRaDeviceModelDto> deviceModelService)
            : base(deviceModelService)
        {
            this.deviceModelService = deviceModelService;
        }

        /// <summary>
        /// Gets the device model list.
        /// </summary>
        /// <returns>An array representing the device models.</returns>
        [HttpGet(Name = "GET LoRaWAN device model list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public override async Task<ActionResult<PaginationResult<DeviceModelDto>>> GetItems([FromQuery] DeviceModelFilter deviceModelFilter)
        {
            var devices = await this.deviceModelService.GetDeviceModels(deviceModelFilter);

            return Ok(devices.Data.Where(d => d.SupportLoRaFeatures));
        }

        /// <summary>
        /// Get the device model details.
        /// </summary>
        /// <param name="id">The devic emodel identifier.</param>
        /// <returns>The device model details.</returns>
        [HttpGet("{id}", Name = "GET LoRaWAN device model")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<ActionResult<LoRaDeviceModelDto>> GetItem(string id)
        {
            return base.GetItem(id);
        }

        /// <summary>
        /// Gets the device model avatar.
        /// </summary>
        /// <param name="id">The device model identifier</param>
        [HttpGet("{id}/avatar", Name = "GET LoRaWAN device model avatar URL")]
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
        /// <returns>The avatar.</returns>
        [HttpPost("{id}/avatar", Name = "POST Update the LoRaWAN device model avatar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<ActionResult<string>> ChangeAvatar(string id)
        {
            return base.ChangeAvatar(id);
        }

        /// <summary>
        /// Deletes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        [HttpDelete("{id}/avatar", Name = "DELETE Remove the LoRaWAN device model avatar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<IActionResult> DeleteAvatar(string id)
        {
            return base.DeleteAvatar(id);
        }

        /// <summary>
        /// Creates the specified device model.
        /// </summary>
        /// <param name="deviceModelDto">The device model.</param>
        /// <returns>The action result.</returns>
        [HttpPost(Name = "POST Create a new LoRaWAN device model")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public override Task<IActionResult> Post(LoRaDeviceModelDto deviceModelDto)
        {
            return base.Post(deviceModelDto);
        }

        /// <summary>
        /// Updates the specified device model.
        /// </summary>
        /// <param name="deviceModelDto">The device model.</param>
        /// <returns>The action result.</returns>
        [HttpPut(Name = "PUT Update the LoRaWAN device model")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<IActionResult> Put(LoRaDeviceModelDto deviceModelDto)
        {
            return base.Put(deviceModelDto);
        }

        /// <summary>
        /// Deletes the specified device model.
        /// </summary>
        /// <param name="id">The device model identifier.</param>
        /// <returns>The action result.</returns>
        [HttpDelete("{id}", Name = "DELETE Remove the LoRaWAN device model")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override Task<IActionResult> Delete(string id)
        {
            return base.Delete(id);
        }
    }
}

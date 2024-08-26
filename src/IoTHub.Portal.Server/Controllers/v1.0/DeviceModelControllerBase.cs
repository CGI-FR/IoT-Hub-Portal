// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System.IO;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;

    public abstract class DeviceModelsControllerBase<TListItemModel, TModel> : ControllerBase
        where TListItemModel : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
#pragma warning disable RCS1158 // Static member in generic type should use a type parameter.
        /// <summary>
        /// The default partition key.
        /// </summary>
        public const string DefaultPartitionKey = "0";
#pragma warning restore RCS1158 // Static member in generic type should use a type parameter.

        private readonly IDeviceModelService<TListItemModel, TModel> deviceModelService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModelsController"/> class.
        /// </summary>
        /// <param name="deviceModelService"></param>
        protected DeviceModelsControllerBase(IDeviceModelService<TListItemModel, TModel> deviceModelService)
        {
            this.deviceModelService = deviceModelService;
        }

        /// <summary>
        /// Gets the device models.
        /// </summary>
        /// <returns>The list of device models.</returns>
        public virtual async Task<ActionResult<PaginationResult<DeviceModelDto>>> GetItems(DeviceModelFilter deviceModelFilter)
        {
            var paginatedDevices = await this.deviceModelService.GetDeviceModels(deviceModelFilter);

            var nextPage = string.Empty;

            if (paginatedDevices.HasNextPage)
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    RouteName = "GET Device Model list",
                    Values = new
                    {
                        deviceModelFilter.SearchText,
                        deviceModelFilter.PageSize,
                        pageNumber = deviceModelFilter.PageNumber + 1,
                        deviceModelFilter.OrderBy
                    }
                });
            }

            return new PaginationResult<DeviceModelDto>
            {
                Items = paginatedDevices.Data,
                TotalItems = paginatedDevices.TotalCount,
                NextPage = nextPage
            };
        }

        /// <summary>
        /// Gets the specified model identifier.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The corresponding model.</returns>
        public virtual async Task<ActionResult<TModel>> GetItem(string id)
        {
            return await this.deviceModelService.GetDeviceModel(id);
        }

        /// <summary>
        /// Gets the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The avatar.</returns>
        public virtual async Task<ActionResult<string>> GetAvatar(string id)
        {
            return Ok(await this.deviceModelService.GetDeviceModelAvatar(id));
        }

        /// <summary>
        /// Changes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="avatar">Avatar as a base64 string</param>
        /// <returns>The avatar.</returns>
        public virtual async Task<ActionResult<string>> ChangeAvatar(string id)
        {
            using var reader = new StreamReader(Request.Body);
            var avatar = await reader.ReadToEndAsync();
            return Ok(await this.deviceModelService.UpdateDeviceModelAvatar(id, avatar));
        }

        /// <summary>
        /// Deletes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        public virtual async Task<IActionResult> DeleteAvatar(string id)
        {
            await this.deviceModelService.DeleteDeviceModelAvatar(id);

            return NoContent();
        }

        /// <summary>
        /// Creates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        public virtual async Task<IActionResult> Post(TModel deviceModel)
        {
            var newDeviceModel = await this.deviceModelService.CreateDeviceModel(deviceModel);
            return CreatedAtAction(nameof(Post), new { id = newDeviceModel.ModelId }, newDeviceModel);
        }

        /// <summary>
        /// Updates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        public virtual async Task<IActionResult> Put(TModel deviceModel)
        {
            await this.deviceModelService.UpdateDeviceModel(deviceModel);

            return Ok();
        }

        /// <summary>
        /// Deletes the specified device model.
        /// </summary>
        /// <param name="id">The device model identifier.</param>
        /// <returns>The action result.</returns>
        public virtual async Task<IActionResult> Delete(string id)
        {
            await this.deviceModelService.DeleteDeviceModel(id);

            return NoContent();
        }
    }
}

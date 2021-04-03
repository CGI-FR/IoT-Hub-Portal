// Copyright (c) Kevin BEAUGRAND. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Identity;
    using AzureIoTHub.Portal.Shared;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Graph;

    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    public class UsersController : Controller
    {
        private readonly GraphServiceClient graphClient;

        private readonly IB2CExtensionHelper extensionHelper;

        public UsersController(GraphServiceClient graphClient, IB2CExtensionHelper extensionHelper)
        {
            this.graphClient = graphClient;
            this.extensionHelper = extensionHelper;
        }

        // GET: UsersController
        public async Task<IActionResult> Get([FromBody] PaginationRequest req)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            var collectionPage = await this.graphClient.Users
                    .Request()
                    .Select($"id,displayName,userPrincipalName,{this.extensionHelper.RoleExtensionName}")
                    .GetAsync();

            var pageResult = new PaginationResult<UserListItem>
            {
                Items = collectionPage.Select(c => new UserListItem
                {
                    Id = c.Id,
                    DisplayName = c.DisplayName,
                    UserName = c.UserPrincipalName,
                    Role = c.AdditionalData[this.extensionHelper.RoleExtensionName].ToString()
                }),
                PageSize = req.PageSize,
                PageIndex = req.PageIndex
            };

            return this.Ok(pageResult);
        }
    }
}

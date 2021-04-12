// Copyright (c) Kevin BEAUGRAND. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Identity;
    using AzureIoTHub.Portal.Shared;
    using AzureIoTHub.Portal.Shared.Security;
    using AzureIoTHub.Portal.Shared.UserManagement;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Graph;

    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    public class UsersController : Controller
    {
        private readonly GraphServiceClient graphClient;

        private readonly IB2CExtensionHelper extensionHelper;

        private readonly IConfiguration configuration;

        public UsersController(GraphServiceClient graphClient, IB2CExtensionHelper extensionHelper, IConfiguration configuration)
        {
            this.graphClient = graphClient;
            this.extensionHelper = extensionHelper;
            this.configuration = configuration;
        }

        [HttpPost]
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

            /*
            foreach (var item in collectionPage)
            {
                await this.graphClient.Users[item.Id].Request().UpdateAsync(new User
                {
                    AdditionalData = new Dictionary<string, object> { { this.extensionHelper.RoleExtensionName, "Admin" } }
                });
            }
            */

            var pageResult = new PaginationResult<UserListItem>
            {
                Items = collectionPage.Select(c => new UserListItem
                {
                    Id = c.Id,
                    DisplayName = c.DisplayName,
                    UserName = c.UserPrincipalName,
                    Role = c.AdditionalData?[this.extensionHelper.RoleExtensionName]?.ToString(),
                    UserType = c.UserType
                }),
                PageSize = req.PageSize,
                PageIndex = req.PageIndex
            };

            return this.Ok(pageResult);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(string userId)
        {
            await this.graphClient.Users[userId].Request().DeleteAsync();

            return this.Ok();
        }

        [HttpPost("invite")]
        public async Task<IActionResult> Invite([FromBody] UserInvitation request)
        {
            var invitation = await this.graphClient.Invitations.Request()
                    .AddAsync(new Invitation
                    {
                        InvitedUserEmailAddress = request.UserEmail,
                        AdditionalData = new Dictionary<string, object> { { this.extensionHelper.RoleExtensionName, request.Role } },
                        SendInvitationMessage = true,
                        InvitedUserDisplayName = request.UserDisplayName,
                        InviteRedirectUrl = this.configuration["PortalUrl"]
                    });

            if (invitation.Status == "Error")
            {
                throw new InvalidOperationException("Faield to create the invitation on Microsoft Graph.");
            }

            await this.graphClient.Users[invitation.InvitedUser.Id].Request().UpdateAsync(new User
            {
                AdditionalData = new Dictionary<string, object> { { this.extensionHelper.RoleExtensionName, request.Role } }
            });
            return this.Ok();
        }
    }
}

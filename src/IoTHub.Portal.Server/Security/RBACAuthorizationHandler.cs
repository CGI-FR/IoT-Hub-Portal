// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Security
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Authorization;

    public class RBACAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider serviceProvider;

        public RBACAuthorizationHandler(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Retrieve the email claim from the user
            var emailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(emailClaim))
            {
                context.Fail();
                return;
            }

            // Check if the user has the required permission
            using var scope = serviceProvider.CreateScope();
            var userManagementService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();
            var accessControlService = scope.ServiceProvider.GetRequiredService<IAccessControlManagementService>();

            // 1. Verify if the authentificated user exists in the database
            var user = await userManagementService.GetOrCreateUserByEmailAsync(emailClaim, context.User);
            if (user == null)
            {
                context.Fail();
                return;
            }

            // 2. Vérify if the user has the required permission
            var hasPermission = await accessControlService.UserHasPermissionAsync(user.PrincipalId, requirement.Permission);
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}

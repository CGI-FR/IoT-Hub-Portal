// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Components.Commons
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.Extensions.Logging;

    public abstract class AuthorizedComponentBase : ComponentBase
    {
        [Inject]
        protected IPermissionsService PermissionsService { get; set; } = default!;

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        [Inject]
        protected ILogger<AuthorizedComponentBase> Logger { get; set; } = default!;

        protected bool IsAuthorized { get; private set; }
        protected bool IsLoading { get; set; } = true;

        /// <summary>
        /// Override this property to specify which permissions are required for this page.
        /// Multiple permissions will be checked with OR logic (user needs at least one).
        /// </summary>
        protected abstract PortalPermissions[] RequiredPermissions { get; }

        protected override async Task OnInitializedAsync()
        {
            await CheckAuthorizationAsync();
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Checks if the current user has at least one of the required permissions.
        /// </summary>
        protected async Task CheckAuthorizationAsync()
        {
            IsLoading = true;

            try
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated != true)
                {
                    Logger.LogWarning("User is not authenticated");
                    IsAuthorized = false;
                    return;
                }

                var userPermissions = await PermissionsService.GetUserPermissions();
                Logger.LogDebug("User has {Count} permissions", userPermissions.Length);

                // Check if user has at least one of the required permissions
                IsAuthorized = RequiredPermissions.Any(required => userPermissions.Contains(required));

                if (!IsAuthorized)
                {
                    Logger.LogWarning(
                        "User does not have required permissions. Required: {Required}, User has: {UserPermissions}",
                        string.Join(", ", RequiredPermissions), string.Join(", ", userPermissions));
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                Logger.LogWarning("Access forbidden when checking permissions");
                IsAuthorized = false;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                Logger.LogWarning("User unauthorized when checking permissions");
                IsAuthorized = false;
            }
            catch (Exception ex)
            {
                // Log the error but set unauthorized to be safe
                Logger.LogError(ex, "Error checking user permissions");
                IsAuthorized = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Checks if the current user has a specific permission.
        /// Useful for conditionally showing/hiding UI elements.
        /// </summary>
        protected async Task<bool> HasPermissionAsync(PortalPermissions permission)
        {
            try
            {
                var userPermissions = await PermissionsService.GetUserPermissions();
                return userPermissions.Contains(permission);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error checking permission {Permission}", permission);
                return false;
            }
        }
    }
}

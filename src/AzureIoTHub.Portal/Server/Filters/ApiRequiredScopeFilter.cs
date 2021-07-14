// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Filters
{
    using System.Linq;
    using AzureIoTHub.Portal.Server.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web.Resource;

    public class ApiRequiredScopeFilter : ActionFilterAttribute
    {
        private IConfigurationSection configurationSection;

        public ApiRequiredScopeFilter(IConfiguration configuration)
        {
            this.configurationSection = configuration.GetSection(MsalSettingsConstants.RootKey);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            /*
            if (context.Controller.GetType().CustomAttributes.Any(c => c.AttributeType == typeof(AllowAnonymousAttribute)))
            {
                return;
            }

            context.HttpContext.VerifyUserHasAnyAcceptedScope(this.configurationSection[MsalSettingsConstants.ScopeName]);
            */
        }
    }
}

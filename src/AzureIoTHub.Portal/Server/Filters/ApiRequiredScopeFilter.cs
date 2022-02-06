// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Filters
{
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Identity.Web.Resource;
    using static AzureIoTHub.Portal.Server.Startup;

    internal class ApiRequiredScopeFilter : ActionFilterAttribute
    {
        private readonly ConfigHandler configuration;

        internal ApiRequiredScopeFilter(ConfigHandler configuration)
        {
            this.configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (context.Controller.GetType().CustomAttributes.Any(c => c.AttributeType == typeof(AllowAnonymousAttribute)))
            {
                return;
            }

            context.HttpContext.VerifyUserHasAnyAcceptedScope(this.configuration.OIDCScope);
        }
    }
}

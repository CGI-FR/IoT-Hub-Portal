// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Filters
{
    using System;
    using AzureIoTHub.Portal.Domain;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class LoRaFeatureActiveFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var configHandler = context.HttpContext.RequestServices.GetService<ConfigHandler>();

            if (!configHandler.IsLoRaEnabled)
            {
                context.Result = new BadRequestObjectResult(context.ModelState)
                {
                    Value = "LoRa features are disabled."
                };
            }
        }
    }
}

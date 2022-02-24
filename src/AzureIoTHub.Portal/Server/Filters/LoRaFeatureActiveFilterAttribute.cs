// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using static AzureIoTHub.Portal.Server.Startup;

namespace AzureIoTHub.Portal.Server.Filters
{
    public class LoRaFeatureActiveFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var configHandler = context.HttpContext.RequestServices.GetService<ConfigHandler>();

            if (configHandler.IsLoRaEnabled == false)
            {
                context.Result = new BadRequestObjectResult(context.ModelState)
                {
                    Value = "LoRa features are disabled."
                };
            }
        }
    }
}

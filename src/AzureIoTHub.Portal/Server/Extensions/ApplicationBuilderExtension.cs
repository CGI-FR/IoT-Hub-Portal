// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Extensions
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    public static class ApplicationBuilderExtension
    {
        public static void UseIfElse(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate,
            Action<IApplicationBuilder> ifConfiguration,
            Action<IApplicationBuilder> elseConfiguration)
        {
            _ = app.UseWhen(predicate, ifConfiguration);
            _ = app.UseWhen(ctx => !predicate(ctx), elseConfiguration);
        }
    }
}

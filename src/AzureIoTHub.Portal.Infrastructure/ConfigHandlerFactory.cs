// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure
{
    using System;
    using AzureIoTHub.Portal.Domain;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    public static class ConfigHandlerFactory
    {
        public static ConfigHandler Create(IHostEnvironment env, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(env, nameof(env));
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            if (env.IsProduction())
            {
                return new ProductionConfigHandler(config);
            }

            return new DevelopmentConfigHandler(config);
        }
    }
}

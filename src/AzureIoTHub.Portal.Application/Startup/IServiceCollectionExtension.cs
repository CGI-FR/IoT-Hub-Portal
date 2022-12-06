// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Startup
{
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            return services.ConfigureMappingProfiles();
        }

        private static IServiceCollection ConfigureMappingProfiles(this IServiceCollection services)
        {
            return services.AddAutoMapper(typeof(IServiceCollectionExtension));
        }
    }
}

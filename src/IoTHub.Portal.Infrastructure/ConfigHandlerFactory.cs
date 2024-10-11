// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure
{
    public static class ConfigHandlerFactory
    {
        public static ConfigHandler Create(IHostEnvironment env, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(env, nameof(env));
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            if (config[ConfigHandlerBase.CloudProviderKey] == null)
            {
                throw new InvalidCloudProviderException(ErrorTitles.InvalidCloudProviderUndefined);
            }

            if (env.IsProduction())
            {
                return config[ConfigHandlerBase.CloudProviderKey] switch
                {
                    CloudProviders.Azure => new ProductionAzureConfigHandler(config),
                    CloudProviders.AWS => new ProductionAWSConfigHandler(config),
                    _ => throw new InvalidCloudProviderException(ErrorTitles.InvalidCloudProviderIncorrect),
                };
            }

            return new DevelopmentConfigHandler(config);
        }
    }
}

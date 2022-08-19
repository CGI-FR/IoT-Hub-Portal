// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.IoTEdgeModule
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ConfigModule
    {
        public ModuleSettings Settings { get; set; }

        public string Type { get; set; }

        [JsonProperty(PropertyName = "env")]
        public IDictionary<string, EnvironmentVariable>? EnvironmentVariables { get; set; }

        public string? Status { get; set; }

        public string? RestarPolicy { get; set; }

        public string? Version { get; set; }

        public ConfigModule()
        {
            Settings = new ModuleSettings();
        }
    }

    public class ModuleSettings
    {
        public string Image { get; set; }

        public string CreateOptions { get; set; }

        public ModuleSettings()
        {
            CreateOptions = "{ }";
        }
    }

    public class EnvironmentVariable
    {
        [JsonProperty(PropertyName = "value")]
        public string EnvValue { get; set; }
    }
}

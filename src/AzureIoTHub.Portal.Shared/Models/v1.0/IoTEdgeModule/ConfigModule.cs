// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.IoTEdgeModule
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ConfigModule
    {
        [JsonProperty(PropertyName = "settings")]
        public ModuleSettings Settings { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "env", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, EnvironmentVariable>? EnvironmentVariables { get; set; }

        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public string? Status { get; set; }

        [JsonProperty(PropertyName = "restartPolicy", NullValueHandling = NullValueHandling.Ignore)]
        public string? RestartPolicy { get; set; }

        public ConfigModule()
        {
            Settings = new ModuleSettings();
        }
    }

    public class ModuleSettings
    {
        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }

        [JsonProperty(PropertyName = "createOptions", NullValueHandling = NullValueHandling.Ignore)]
        public string CreateOptions { get; set; }
    }

    public class EnvironmentVariable
    {
        [JsonProperty(PropertyName = "value")]
        public string EnvValue { get; set; }
    }
}

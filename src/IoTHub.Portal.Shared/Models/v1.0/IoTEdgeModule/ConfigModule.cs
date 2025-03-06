// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace IoTHub.Portal.Shared.Models.v10.IoTEdgeModule
{
    public class ConfigModule
    {
        [JsonPropertyName("settings")]
        [JsonProperty(PropertyName = "settings")]
        public ModuleSettings Settings { get; set; }

        [JsonPropertyName("type")]
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("env")]
        [JsonProperty(PropertyName = "env", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, EnvironmentVariable>? EnvironmentVariables { get; set; }

        [JsonPropertyName("status")]
        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public string? Status { get; set; }

        [JsonPropertyName("restartPolicy")]
        [JsonProperty(PropertyName = "restartPolicy", NullValueHandling = NullValueHandling.Ignore)]
        public string? RestartPolicy { get; set; }

        public ConfigModule()
        {
            Settings = new ModuleSettings();
        }
    }

    public class ModuleSettings
    {
        [JsonPropertyName("image")]
        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; } = default!;

        [JsonPropertyName("createOptions")]
        [JsonProperty(PropertyName = "createOptions", NullValueHandling = NullValueHandling.Ignore)]
        public string CreateOptions { get; set; } = default!;

        [JsonPropertyName("startupOrder")]
        [JsonProperty(PropertyName = "startupOrder", NullValueHandling = NullValueHandling.Ignore)]
        public int StartupOrder { get; set; } = 0;
    }

    public class EnvironmentVariable
    {
        [JsonPropertyName("value")]
        [JsonProperty(PropertyName = "value")]
        public string EnvValue { get; set; } = default!;
    }
}

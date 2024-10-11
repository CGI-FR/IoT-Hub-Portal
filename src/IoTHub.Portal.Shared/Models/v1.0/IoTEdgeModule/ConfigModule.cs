// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace IoTHub.Portal.Shared.Models.v10.IoTEdgeModule
{
    public class ConfigModule
    {
        [JsonPropertyName("settings")]
        public ModuleSettings Settings { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("env")]
        //,  NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, EnvironmentVariable>? EnvironmentVariables { get; set; }

        [JsonPropertyName("status")]/*, NullValueHandling = NullValueHandling.Ignore)]*/
        public string? Status { get; set; }

        [JsonPropertyName("restartPolicy")]/*, NullValueHandling = NullValueHandling.Ignore)]*/
        public string? RestartPolicy { get; set; }

        public ConfigModule()
        {
            Settings = new ModuleSettings();
        }
    }

    public class ModuleSettings
    {
        [JsonPropertyName("image")]
        public string Image { get; set; } = default!;

        [JsonPropertyName("createOptions")]/*, NullValueHandling = NullValueHandling.Ignore)]*/
        public string CreateOptions { get; set; } = default!;
    }

    public class EnvironmentVariable
    {
        [JsonPropertyName("value")]
        public string EnvValue { get; set; } = default!;
    }
}

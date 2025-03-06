// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10.IoTEdgeModule
{
    public class EdgeAgentPropertiesDesired
    {
        [JsonPropertyName("modules")]
        [JsonProperty(PropertyName = "modules")]
        public IDictionary<string, ConfigModule> Modules { get; set; }

        [JsonPropertyName("runtime")]
        [JsonProperty(PropertyName = "runtime")]
        public Runtime Runtime { get; set; }

        [JsonPropertyName("schemaVersion")]
        [JsonProperty(PropertyName = "schemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonPropertyName("systemModules")]
        [JsonProperty(PropertyName = "systemModules")]
        public SystemModules SystemModules { get; set; }

        public EdgeAgentPropertiesDesired()
        {
            Modules = new Dictionary<string, ConfigModule>();
            Runtime = new Runtime();
            SystemModules = new SystemModules();
            SchemaVersion = "1.1";
        }
    }

    public class Runtime
    {
        [JsonPropertyName("settings")]
        [JsonProperty(PropertyName = "settings")]
        public RuntimeSettings Settings { get; set; }

        [JsonPropertyName("type")]
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        public Runtime()
        {
            Settings = new RuntimeSettings();
            Type = "docker";
        }
    }

    public class RuntimeSettings
    {
        [JsonPropertyName("minDockerVersion")]
        [JsonProperty(PropertyName = "minDockerVersion")]
        public string MinDockerVersion { get; set; }

        public RuntimeSettings()
        {
            MinDockerVersion = "v1.25";
        }
    }

    public class SystemModules
    {
        [JsonPropertyName("edgeAgent")]
        [JsonProperty(PropertyName = "edgeAgent")]
        public ConfigModule EdgeAgent { get; set; }

        [JsonPropertyName("edgeHub")]
        [JsonProperty(PropertyName = "edgeHub")]
        public ConfigModule EdgeHub { get; set; }

        public SystemModules()
        {
            EdgeAgent = new ConfigModule()
            {
                Settings = new ModuleSettings { Image = "mcr.microsoft.com/azureiotedge-agent:1.1" },
                Type = "docker",
                EnvironmentVariables = new Dictionary<string, EnvironmentVariable>()
            };

            EdgeHub = new ConfigModule()
            {
                Type = "docker",
                Status = "running",
                RestartPolicy = "always",
                Settings = new ModuleSettings
                {
                    Image = "mcr.microsoft.com/azureiotedge-hub:1.1",
                    CreateOptions = /*lang=json,strict*/ "{\"HostConfig\":{\"PortBindings\":{\"443/tcp\":[{\"HostPort\":\"443\"}],\"5671/tcp\":[{\"HostPort\":\"5671\"}],\"8883/tcp\":[{\"HostPort\":\"8883\"}]}}}"
                },
                EnvironmentVariables = new Dictionary<string, EnvironmentVariable>()
            };
        }
    }
}

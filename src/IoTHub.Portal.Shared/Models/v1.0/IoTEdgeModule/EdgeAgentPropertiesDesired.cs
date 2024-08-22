// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0.IoTEdgeModule
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class EdgeAgentPropertiesDesired
    {
        [JsonProperty(PropertyName = "modules")]
        public IDictionary<string, ConfigModule> Modules { get; set; }

        [JsonProperty(PropertyName = "runtime")]
        public Runtime Runtime { get; set; }

        [JsonProperty(PropertyName = "schemaVersion")]
        public string SchemaVersion { get; set; }

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
        [JsonProperty(PropertyName = "settings")]
        public RuntimeSettings Settings { get; set; }

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
        [JsonProperty(PropertyName = "minDockerVersion")]
        public string MinDockerVersion { get; set; }

        public RuntimeSettings()
        {
            MinDockerVersion = "v1.25";
        }
    }

    public class SystemModules
    {
        [JsonProperty(PropertyName = "edgeAgent")]
        public ConfigModule EdgeAgent { get; set; }

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

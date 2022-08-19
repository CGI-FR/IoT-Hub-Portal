// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.IoTEdgeModule
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class EdgeAgentPropertiesDesired
    {
        public IDictionary<string, ConfigModule> Modules { get; set; }

        public Runtime Runtime { get; set; }

        public string SchemaVersion { get; set; }

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
        public RuntimeSettings Settings { get; set; }

        public string Type { get; set; }

        public Runtime()
        {
            Settings = new RuntimeSettings();
            Type = "docker";
        }
    }

    public class RuntimeSettings
    {
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
                Type = "docker"
            };

            EdgeHub = new ConfigModule()
            {
                Type = "docker",
                Status = "running",
                RestarPolicy = "always",
                Settings = new ModuleSettings
                {
                    Image = "mcr.microsoft.com/azureiotedge-hub:1.1",
                    CreateOptions = "\\{\"HostConfig\":{\"PortBindings\":{\"443/tcp\":[{\"HostPort\":\"443\"}],\"5671/tcp\":[{\"HostPort\":\"5671\"}],\"8883/tcp\":[{\"HostPort\":\"8883\"}]}}}"
                }
            };
        }
    }
}

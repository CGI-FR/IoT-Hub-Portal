// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    public class EdgeModelSystemModule
    {
        public string Name { get; set; }

        public string Image { get; set; } = default!;

        public string ContainerCreateOptions { get; set; } = default!;

        public int StartupOrder { get; set; }

        public List<IoTEdgeModuleEnvironmentVariable> EnvironmentVariables { get; set; }

        public EdgeModelSystemModule(string name)
        {
            Name = name;
            EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>();
        }
    }
}

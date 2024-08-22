// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System.Collections.Generic;

    public class EdgeModelSystemModule
    {
        public string Name { get; internal set; }

        public string ImageUri { get; set; } = default!;

        public string ContainerCreateOptions { get; set; } = default!;

        public List<IoTEdgeModuleEnvironmentVariable> EnvironmentVariables { get; set; }

        public EdgeModelSystemModule(string name)
        {
            Name = name;
            EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>();
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    using System.Collections.Generic;

    public class EdgeModelSystemModule
    {
        public string Name { get; internal set; }

        public string ImageUri { get; set; }

        public string ContainerCreateOptions { get; set; }

        public List<IoTEdgeModuleEnvironmentVariable> EnvironmentVariables { get; set; }

        public EdgeModelSystemModule(string name)
        {
            Name = name;
            EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>();
        }
    }
}

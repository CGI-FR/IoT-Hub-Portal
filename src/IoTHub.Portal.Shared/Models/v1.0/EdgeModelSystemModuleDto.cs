// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    using System.Collections.Generic;

    public class EdgeModelSystemModuleDto
    {
        public string Name { get; internal set; }

        public string ImageUri { get; set; } = default!;

        public string ContainerCreateOptions { get; set; } = default!;

        public List<IoTEdgeModuleEnvironmentVariableDto> EnvironmentVariables { get; set; }

        public EdgeModelSystemModuleDto(string name)
        {
            Name = name;
            EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariableDto>();
        }
    }
}

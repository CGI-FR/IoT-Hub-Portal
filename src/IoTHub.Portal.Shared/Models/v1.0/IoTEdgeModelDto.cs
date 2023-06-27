// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10
{
    using System.Collections.Generic;
    using IoTHub.Portal.Shared.Models.v10;

    public class IoTEdgeModelDto : IoTEdgeModelListItemDto
    {
        /// <summary>
        /// The device model module list.
        /// </summary>
        public List<IoTEdgeModuleDto> EdgeModules { get; set; } = new List<IoTEdgeModuleDto>();

        public List<IoTEdgeRouteDto> EdgeRoutes { get; set; } = new List<IoTEdgeRouteDto>();

        public List<EdgeModelSystemModuleDto> SystemModules { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        public new List<LabelDto> Labels { get; set; } = new();

        public IoTEdgeModelDto()
        {
            SystemModules = new List<EdgeModelSystemModuleDto>
            {
                new EdgeModelSystemModuleDto("edgeAgent"),
                new EdgeModelSystemModuleDto("edgeHub")
            };
        }
    }
}

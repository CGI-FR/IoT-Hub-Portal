// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System.Collections.Generic;

    public class IoTEdgeModel : IoTEdgeModelListItem
    {
        /// <summary>
        /// The device model module list.
        /// </summary>
        public List<IoTEdgeModule> EdgeModules { get; set; } = new List<IoTEdgeModule>();

        public List<IoTEdgeRoute> EdgeRoutes { get; set; } = new List<IoTEdgeRoute>();

        public List<EdgeModelSystemModule> SystemModules { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        public new List<LabelDto> Labels { get; set; } = new();

        public IoTEdgeModel()
        {
            SystemModules = new List<EdgeModelSystemModule>
            {
                new EdgeModelSystemModule("edgeAgent"),
                new EdgeModelSystemModule("edgeHub")
            };
        }
    }
}

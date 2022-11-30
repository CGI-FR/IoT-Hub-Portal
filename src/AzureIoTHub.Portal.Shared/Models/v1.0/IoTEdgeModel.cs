// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Shared.Models.v10;

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
        public List<LabelDto> Labels { get; set; } = new();

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

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System.Collections.Generic;

    public class IoTEdgeModel : IoTEdgeModelListItem
    {
        /// <summary>
        /// The device model module list.
        /// </summary>
        public List<IoTEdgeModule> EdgeModules { get; set; } = new List<IoTEdgeModule>();

        public List<IoTEdgeRoute> EdgeRoutes { get; set; } = new List<IoTEdgeRoute>();
    }
}

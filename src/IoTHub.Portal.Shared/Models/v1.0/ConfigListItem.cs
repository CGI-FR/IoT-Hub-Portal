// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IoT Edge configuration list item.
    /// </summary>
    public class ConfigListItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigListItem"/> class.
        /// </summary>
        public ConfigListItem()
        {
            Modules = new List<IoTEdgeModule>();
        }

        /// <summary>
        /// The IoT Edge configuration identifier.
        /// </summary>
        public string ConfigurationID { get; set; } = default!;

        /// <summary>
        /// The IoT Edge configuration target conditions.
        /// </summary>
        public string Conditions { get; set; } = default!;

        /// <summary>
        /// The IoT Edge configuration targeted metrics.
        /// </summary>
        public long MetricsTargeted { get; set; }

        /// <summary>
        /// The IoT Edge configuration applied metrics.
        /// </summary>
        public long MetricsApplied { get; set; }

        /// <summary>
        /// The IoT Edge configuration success metrics.
        /// </summary>
        public long MetricsSuccess { get; set; }

        /// <summary>
        /// The IoT Edge configuration failure metrics.
        /// </summary>
        public long MetricsFailure { get; set; }

        /// <summary>
        /// The IoT Edge configuration priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The IoT Edge configuration creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        ///<summary>
        /// The IoT Edge modules configuration.
        /// </summary>
        public IReadOnlyCollection<IoTEdgeModule> Modules { get; set; }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;

    public class ConfigurationMetrics
    {
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
        /// The IoT Edge configuration creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }
    }
}

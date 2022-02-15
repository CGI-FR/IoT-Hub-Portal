// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    using System;
    using System.Collections.Generic;

    public class ConfigListItem
    {
        public ConfigListItem()
        {
            this.Modules = new List<GatewayModule>();
        }
        ///<summary>
        /// The command configurationId.
        /// </summary>
        public string ConfigurationID { get; set; }

        ///<summary>
        /// The command conditions.
        /// </summary>
        public string Conditions { get; set; }

        ///<summary>
        /// The command metricsTargered.
        /// </summary>
        public long MetricsTargeted { get; set; }

        ///<summary>
        /// The command metricsApplied.
        /// </summary>
        public long MetricsApplied { get; set; }

        ///<summary>
        /// The command metricsSuccess.
        /// </summary>
        public long MetricsSuccess { get; set; }

        ///<summary>
        /// The command metricsFailure.
        /// </summary>
        public long MetricsFailure { get; set; }

        ///<summary>
        /// The command priority.
        /// </summary>
        public int Priority { get; set; }

        ///<summary>
        /// The device creationDate.
        /// </summary>
        public DateTime CreationDate { get; set; }

        ///<summary>
        /// The gateway modules.
        /// </summary>
        public List<GatewayModule> Modules { get; set; }
    }
}

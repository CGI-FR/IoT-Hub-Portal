﻿// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.Collections.Generic;

    public class ConfigListItem
    {
        public ConfigListItem()
        {
            this.Modules = new List<GatewayModule>();
        }

        public string ConfigurationID { get; set; }

        public string Conditions { get; set; }

        public long MetricsTargeted { get; set; }

        public long MetricsApplied { get; set; }

        public long MetricsSuccess { get; set; }

        public long MetricsFailure { get; set; }

        public int Priority { get; set; }

        public DateTime CreationDate { get; set; }

        public List<GatewayModule> Modules { get; set; }
    }
}

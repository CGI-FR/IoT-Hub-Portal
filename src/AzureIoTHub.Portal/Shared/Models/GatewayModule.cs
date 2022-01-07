﻿// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GatewayModule
    {
        public string ModuleName { get; set; }

        public string Version { get; set; }

        public string Status { get; set; }

        public Dictionary<string, string> EnvironmentVariables { get; set; }

        public Dictionary<string, string> ModuleIdentityTwinSettings { get; set; }
    }
}

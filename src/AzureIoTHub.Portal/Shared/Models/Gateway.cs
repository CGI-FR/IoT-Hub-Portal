// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Gateway
    {
        public string DeviceId { get; set; }

        public string Type { get; set; }

        public string Status { get; set; }

        public string RuntimeResponse { get; set; }

        public int NbDevices { get; set; }

        public int NbModule { get; set; }

        public string Environement { get; set; }

        public List<GatewayModule> Modules { get; set; }

        public Gateway()
        {
            this.Modules = new List<GatewayModule>();
        }
    }
}

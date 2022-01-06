// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Gateway
    {
        [Required]
        public string DeviceId { get; set; }

        public string SymmetricKey { get; set; }

        public string Connection_state { get; set; }

        public string Scope { get; set; }

        public string EndPoint { get; set; }

        [Required]
        public string Type { get; set; }

        public string Status { get; set; }

        public string RuntimeResponse { get; set; }

        public int NbDevices { get; set; }

        public int NbModule { get; set; }

        public string Environment { get; set; }

        public ConfigItem LastDeployment { get; set; }

        public List<GatewayModule> Modules { get; set; }

        public Gateway()
        {
            this.Modules = new List<GatewayModule>();
            this.LastDeployment = new ConfigItem();
        }
    }
}

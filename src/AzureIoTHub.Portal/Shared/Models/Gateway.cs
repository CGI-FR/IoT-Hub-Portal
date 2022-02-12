// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Gateway
    {
        [Required]
        public string DeviceId { get; set; }

        public string SymmetricKey { get; set; }

        public string ConnectionState { get; set; }

        public string Scope { get; set; }

        public string EndPoint { get; set; }

        [Required]
        public string Type { get; set; }

        public string Status { get; set; }

        public string RuntimeResponse { get; set; }

        public int NbDevices { get; set; }

        public int NbModules { get; set; }

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

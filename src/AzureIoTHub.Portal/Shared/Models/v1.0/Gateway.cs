// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Gateway
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        [Required]
        public string DeviceId { get; set; }

        /// <summary>
        /// Creates a new symmetric key.
        /// </summary>
        public string SymmetricKey { get; set; }

        /// <summary>
        /// Gets connection state.
        /// </summary>
        public string ConnectionState { get; set; }

        /// <summary>
        /// Called the scope of a variable.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Create the binding to be used par le service.
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// Initialize a new instance of the type class.
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// The device status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// A methode response.
        /// </summary>
        public string RuntimeResponse { get; set; }

        
        public int NbDevices { get; set; }

        
        public int NbModules { get; set; }

        /// <summary>
        /// The variable environment.
        /// </summary>
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

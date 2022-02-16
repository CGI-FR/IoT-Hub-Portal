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
        [Required(ErrorMessage = "The device identifier is required.")]
        public string DeviceId { get; set; }

        /// <summary>
        /// The device symmetric key.
        /// </summary>
        public string SymmetricKey { get; set; }

        /// <summary>
        /// The device connection state.
        /// </summary>
        public string ConnectionState { get; set; }

        /// <summary>
        /// The device scope.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// The device end point.
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// The device type.
        /// </summary>
        [Required(ErrorMessage = "The device type is required.")]
        public string Type { get; set; }

        /// <summary>
        /// The device status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The device runtime response.
        /// </summary>
        public string RuntimeResponse { get; set; }

        /// <summary>
        /// The device nbdevices.
        /// </summary>
        public int NbDevices { get; set; }

        /// <summary>
        /// The device nbmodules.
        /// </summary>
        public int NbModules { get; set; }

        /// <summary>
        /// The device environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// The device nbdevices.
        /// </summary>
        public ConfigItem LastDeployment { get; set; }

        /// <summary>
        /// The device modules.
        /// </summary>
        public List<GatewayModule> Modules { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gateway"/> class.
        /// </summary>
        public Gateway()
        {
            this.Modules = new List<GatewayModule>();
            this.LastDeployment = new ConfigItem();
        }
    }
}

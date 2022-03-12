// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class IoTEdgeModule
    {
        /// <summary>
        /// The module name.
        /// </summary>
        [Required(ErrorMessage = "The device model name is required.")]
        public string ModuleName { get; set; }

        /// <summary>
        /// The module configuration version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The module status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The module environment variables.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// The module identity twin settings.
        /// </summary>
        public Dictionary<string, string> ModuleIdentityTwinSettings { get; set; }
    }
}

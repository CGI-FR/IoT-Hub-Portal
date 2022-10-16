// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AzureIoTHub.Portal.Shared.Models.v10;

    /// <summary>
    /// IoT Edge module.
    /// </summary>
    public class IoTEdgeModule
    {
        /// <summary>
        /// The module name.
        /// </summary>
        [Required(ErrorMessage = "The device model name is required.")]
        public string ModuleName { get; set; }

        /// <summary>
        /// the device image URI.
        /// </summary>
        [Required(ErrorMessage = "The device image uri is required.")]
        public string ImageURI { get; set; }

        public string ContainerCreateOptions { get; set; }

        /// <summary>
        /// The module status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The module environment variables.
        /// </summary>
        public List<IoTEdgeModuleEnvironmentVariable> EnvironmentVariables { get; set; } = new List<IoTEdgeModuleEnvironmentVariable>();

        /// <summary>
        /// The module identity twin settings.
        /// </summary>
        public List<IoTEdgeModuleTwinSetting> ModuleIdentityTwinSettings { get; set; } = new List<IoTEdgeModuleTwinSetting>();

        /// <summary>
        /// The module commands.
        /// </summary>
        public List<IoTEdgeModuleCommand> Commands { get; set; } = new List<IoTEdgeModuleCommand>();
    }
}

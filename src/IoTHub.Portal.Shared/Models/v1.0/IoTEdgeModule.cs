// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// IoT Edge module.
    /// </summary>
    public class IoTEdgeModule
    {
        /// <summary>
        /// The module name, only used for AWS IoT Greengrass.
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// The module name.
        /// </summary>
        [Required(ErrorMessage = "The device model name is required.")]
        public string ModuleName { get; set; } = default!;

        /// <summary>
        /// the device image URI.
        /// </summary>
        [Required(ErrorMessage = "The device image uri is required.")]
        public string ImageURI { get; set; } = default!;

        public string ContainerCreateOptions { get; set; } = default!;

        /// <summary>
        /// The module status.
        /// </summary>
        public string Status { get; set; } = default!;

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

        public string Version { get; set; } = default!;
    }
}

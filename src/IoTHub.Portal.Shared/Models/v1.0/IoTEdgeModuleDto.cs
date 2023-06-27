// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using IoTHub.Portal.Shared.Models.v10;

    /// <summary>
    /// IoT Edge module.
    /// </summary>
    public class IoTEdgeModuleDto
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
        public List<IoTEdgeModuleEnvironmentVariableDto> EnvironmentVariables { get; set; } = new List<IoTEdgeModuleEnvironmentVariableDto>();

        /// <summary>
        /// The module identity twin settings.
        /// </summary>
        public List<IoTEdgeModuleTwinSettingDto> ModuleIdentityTwinSettings { get; set; } = new List<IoTEdgeModuleTwinSettingDto>();

        /// <summary>
        /// The module commands.
        /// </summary>
        public List<IoTEdgeModuleCommandDto> Commands { get; set; } = new List<IoTEdgeModuleCommandDto>();

        [Required(ErrorMessage = "The component version is required.", AllowEmptyStrings = true)]
        public string Version { get; set; } = default!;
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    using System.ComponentModel.DataAnnotations;

    public class IoTEdgeModuleEnvironmentVariable
    {
        /// <summary>
        /// The module environment variable name
        /// </summary>
        [RegularExpression("^[^\\.^\\$^\\#$\\ ]{1,128}$", ErrorMessage = "Variable name should be less than 128 characters and must not contain Control Characters, '.', '$', '#', or ' '.")]
        public string Name { get; set; }

        /// <summary>
        /// The module environment variable value
        /// </summary>
        public string Value { get; set; }
    }
}

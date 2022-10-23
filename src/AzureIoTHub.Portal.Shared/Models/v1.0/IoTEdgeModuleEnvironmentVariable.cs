// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    public class IoTEdgeModuleEnvironmentVariable
    {
        /// <summary>
        /// The module environment variable name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The module environment variable value
        /// </summary>
        public string Value { get; set; }
    }
}

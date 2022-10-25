// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    public class IoTEdgeModuleCommand
    {
        /// <summary>
        /// The command name
        /// </summary>
        public string Name { get; set; }

        public string EdgeDeviceModelId { get; set; }

        public string CommandId { get; set; }

        public string ModuleName { get; set; }
    }
}

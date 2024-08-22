// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    public class IoTEdgeModuleCommand
    {
        /// <summary>
        /// The command name
        /// </summary>
        public string Name { get; set; } = default!;

        public string EdgeDeviceModelId { get; set; } = default!;

        public string CommandId { get; set; } = default!;

        public string ModuleName { get; set; } = default!;
    }
}

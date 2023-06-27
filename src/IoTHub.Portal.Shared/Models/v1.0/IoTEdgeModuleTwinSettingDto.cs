// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    public class IoTEdgeModuleTwinSettingDto
    {
        /// <summary>
        /// The module identity twin setting name
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// The module identity twin setting value
        /// </summary>
        public string Value { get; set; } = default!;
    }
}

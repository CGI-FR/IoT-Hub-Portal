// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    /// <summary>
    /// Cloud to Device message result.
    /// </summary>
    public class C2Dresult
    {
        /// <summary>
        /// The C2D result payload.
        /// </summary>
        public string Payload { get; set; } = default!;

        /// <summary>
        /// The C2D status.
        /// </summary>
        public int Status { get; set; }
    }
}

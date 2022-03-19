// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10.LoRaWAN
{
    /// <summary>
    /// LoRAWAN Command
    /// </summary>
    public class Command
    {
        /// <summary>
        /// The command identifier.
        /// </summary>
        public string CommandId { get; set; }

        /// <summary>
        /// The frame.
        /// </summary>
        public string Frame { get; set; }
    }
}

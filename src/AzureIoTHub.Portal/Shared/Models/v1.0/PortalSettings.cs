// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    public class PortalSettings
    {
        /// <summary>
        /// A value indicating whether the LoRa features are acticated.
        /// </summary>
        public bool IsLoRaSupported { get; set; }

        /// <summary>
        /// The portal version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The poral name.
        /// </summary>
        public string PortalName { get; set; }
    }
}

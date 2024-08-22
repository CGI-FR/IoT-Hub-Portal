// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    /// <summary>
    /// Portal Settings.
    /// </summary>
    public class PortalSettings
    {
        /// <summary>
        /// A value indicating whether the LoRa features are acticated.
        /// </summary>
        public bool IsLoRaSupported { get; set; }

        /// <summary>
        /// The portal version.
        /// </summary>
        public string Version { get; set; } = default!;

        /// <summary>
        /// The poral name.
        /// </summary>
        public string PortalName { get; set; } = default!;

        /// <summary>
        /// Copyright Year
        /// </summary>
        public string CopyrightYear { get; set; } = default!;

        public bool IsIdeasFeatureEnabled { get; set; }

        /// <summary>
        /// The poral cloudProvider.
        /// </summary>
        public string CloudProvider { get; set; } = default!;
    }
}

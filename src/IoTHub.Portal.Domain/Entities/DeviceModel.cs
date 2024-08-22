// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using Base;
    using Portal.Shared.Models.v1._0.LoRaWAN;

    public class DeviceModel : EntityBase
    {
        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public bool IsBuiltin { get; set; }

        public bool SupportLoRaFeatures { get; set; }

        public bool? UseOTAA { get; set; }

        public int? PreferredWindow { get; set; }

        public DeduplicationMode? Deduplication { get; set; }

        public ClassType ClassType { get; set; }

        public bool? ABPRelaxMode { get; set; }

        public bool? Downlink { get; set; }

        public int? KeepAliveTimeout { get; set; }

        public int? RXDelay { get; set; }

        public string? SensorDecoder { get; set; }

        public string? AppEUI { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        public ICollection<Label> Labels { get; set; } = default!;
    }
}

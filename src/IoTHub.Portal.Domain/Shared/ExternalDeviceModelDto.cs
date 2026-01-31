// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Shared
{
    public class ExternalDeviceModelDto
    {
        public string Id { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string Description { get; set; } = default!;

        public bool IsBuiltin { get; set; }

        public bool SupportLoRaFeatures { get; set; }
    }
}

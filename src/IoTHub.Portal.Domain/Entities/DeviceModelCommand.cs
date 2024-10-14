// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    public class DeviceModelCommand : EntityBase
    {
        public string Name { get; set; } = default!;

        public string Frame { get; set; } = default!;

        public bool Confirmed { get; set; }

        public int Port { get; set; } = 1;

        public bool IsBuiltin { get; set; }

        public string DeviceModelId { get; set; } = default!;
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using Base;

    public class DeviceModelCommand : EntityBase
    {
        public string Name { get; set; }

        public string Frame { get; set; }

        public bool Confirmed { get; set; }

        public int Port { get; set; } = 1;

        public bool IsBuiltin { get; set; }

        public string DeviceModelId { get; set; }
    }
}

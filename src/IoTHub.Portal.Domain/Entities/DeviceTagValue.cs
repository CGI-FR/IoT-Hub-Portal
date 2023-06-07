// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using Base;

    public class DeviceTagValue : EntityBase
    {
        public string Name { get; set; } = default!;

        public string Value { get; set; } = default!;
    }
}

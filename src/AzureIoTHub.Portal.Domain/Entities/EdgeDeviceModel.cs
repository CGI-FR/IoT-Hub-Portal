// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using Base;

    public class EdgeDeviceModel : EntityBase
    {
        public string Name { get; set; }

        public string? Description { get; set; }

    }
}

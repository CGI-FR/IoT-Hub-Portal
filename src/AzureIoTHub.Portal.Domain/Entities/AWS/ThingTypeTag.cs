// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities.AWS
{
    using AzureIoTHub.Portal.Domain.Base;

    public class ThingTypeTag : EntityBase
    {
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;

    }
}

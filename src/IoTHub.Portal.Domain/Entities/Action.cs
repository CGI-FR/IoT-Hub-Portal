// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;

    public class Action : EntityBase
    {
        public string Name { get; set; } = default!;
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Base
{
    public abstract class EntityBase
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}

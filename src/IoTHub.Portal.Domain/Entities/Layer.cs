// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;

    public class Layer : EntityBase
    {
        /// <summary>
        /// The Layer friendly name.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Where level is.
        /// </summary>
        public string Father { get; set; } = default!;

        /// <summary>
        /// The planning associat with the level.
        /// </summary>
        public string Planning { get; set; } = default!;

        /// <summary>
        /// The planning has a subLayer.
        /// </summary>
        public bool hasSub { get; set; } = default!;

    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    using System;

    //using System.ComponentModel.DataAnnotations;

    public class LayerDto
    {
        /// <summary>
        /// The level auto ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The level friendly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Where level is.
        /// </summary>
        public string? Father { get; set; }

        /// <summary>
        /// The planning associat with the level.
        /// </summary>
        public string Planning { get; set; }

        /// <summary>
        /// The planning has a subLayer.
        /// </summary>
        public bool hasSub { get; set; }
    }
}

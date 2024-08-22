// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System.ComponentModel.DataAnnotations;

    public class IoTEdgeRoute
    {
        /// <summary>
        /// The route name.
        /// </summary>
        [Required(ErrorMessage = "The route should have a name.")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// The route value.
        /// Eg : FROM /messages/* INTO $upstream.
        /// </summary>
        [RegularExpression(@"^(?i)FROM [\S]+( WHERE (NOT )?[\S]+)? INTO [\S]+$", ErrorMessage = "Route should be 'FROM <source> (WHERE <condition>) INTO <sink>'")]
        [Required(ErrorMessage = "The route should have a value.")]
        public string Value { get; set; } = default!;

        /// <summary>
        /// The route priority
        /// </summary>
        [Range(0, 9)]
        public int? Priority { get; set; } = null;

        /// <summary>
        /// The route time to live (secs)
        /// </summary>
        [Range(0, uint.MaxValue)]
        public uint? TimeToLive { get; set; } = null;

    }
}

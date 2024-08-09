// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;

    public class Schedule : EntityBase
    {
        /// <summary>
        /// The schedule friendly name.
        /// </summary>
        public string Start { get; set; } = default!;

        /// <summary>
        /// Where schedule is.
        /// </summary>
        public string End { get; set; } = default!;

        /// <summary>
        /// The planning associat with the schedule.
        /// </summary>
        public string CommandId { get; set; } = default!;

        /// <summary>
        /// The planning associat with the schedule.
        /// </summary>
        public string PlanningId { get; set; } = default!;

    }
}

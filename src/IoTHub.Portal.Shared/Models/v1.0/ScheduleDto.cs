// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    using System;

    //using System.ComponentModel.DataAnnotations;

    public class ScheduleDto
    {
        /// <summary>
        /// The room auto ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The schedule friendly name.
        /// </summary>
        public int Start { get; set; } = default!;

        /// <summary>
        /// Where schedule is.
        /// </summary>
        public int End { get; set; } = default!;

        /// <summary>
        /// The planning associat with the schedule.
        /// </summary>
        public int Temperature { get; set; } = default!;

        /// <summary>
        /// The planning associat with the schedule.
        /// </summary>
        public string Planning { get; set; } = default!;
    }
}

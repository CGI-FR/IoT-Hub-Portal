// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    using System;

    //using System.ComponentModel.DataAnnotations;

    public class PlanningDto
    {
        /// <summary>
        /// The room auto ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The planning friendly name.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Where planning start.
        /// </summary>
        public int Start { get; set; } = default!;

        /// <summary>
        /// Where planning end.
        /// </summary>
        public int End { get; set; } = default!;

        /// <summary>
        /// How much it repeat.
        /// </summary>
        public int Frequency { get; set; } = default!;

        /// <summary>
        /// When planning is used
        /// </summary>
        public string DayOff { get; set; } = default!;

        /// <summary>
        /// Day off temperature.
        /// </summary>
        public int TemperatureOff { get; set; } = default!;
    }
}

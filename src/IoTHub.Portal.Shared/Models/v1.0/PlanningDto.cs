// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    public class PlanningDto
    {
        /// <summary>
        /// The level auto ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The planning friendly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Where planning start.
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// Where planning end.
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// How much it repeat.
        /// </summary>
        public bool Frequency { get; set; }

        /// <summary>
        /// When planning is used
        /// </summary>
        public DaysEnumFlag.DaysOfWeek DayOff { get; set; }

        /// <summary>
        /// Day off command.
        /// </summary>
        public string CommandId { get; set; }

        /// <summary>
        /// The device model identifier associated with this planning.
        /// </summary>
        public string? DeviceModelId { get; set; }
    }
}

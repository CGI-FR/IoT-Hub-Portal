// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    public static class PlanningScheduleValidator
    {
        private static readonly TimeSpan FirstSlotStart = TimeSpan.Zero;
        private static readonly TimeSpan LastSlotEnd = new(23, 59, 0);
        private const string TimeFormat = @"hh\:mm";

        public static string? ValidateTimeline(IReadOnlyCollection<ScheduleDto>? schedules)
        {
            if (schedules is null || schedules.Count == 0)
            {
                return "At least one business-day slot is required.";
            }

            var slots = new List<(TimeSpan Start, TimeSpan End)>();
            foreach (var schedule in schedules)
            {
                if (schedule is null || !TryParseTime(schedule.Start, out var start) || !TryParseTime(schedule.End, out var end))
                {
                    return "Slot times must use the HH:mm format.";
                }

                if (end <= start)
                {
                    return "Each slot's end time must be after its start time.";
                }

                slots.Add((start, end));
            }

            slots.Sort((left, right) => left.Start.CompareTo(right.Start));

            if (slots[0].Start != FirstSlotStart)
            {
                return "The first slot must start at 00:00.";
            }

            for (var index = 1; index < slots.Count; index++)
            {
                if (slots[index].Start != slots[index - 1].End)
                {
                    return "Business-day slots must form a continuous timeline.";
                }
            }

            if (slots[^1].End != LastSlotEnd)
            {
                return "The last slot must end at 23:59.";
            }

            return null;
        }

        private static bool TryParseTime(string value, out TimeSpan time)
        {
            return TimeSpan.TryParseExact(value, TimeFormat, System.Globalization.CultureInfo.InvariantCulture, out time);
        }
    }
}

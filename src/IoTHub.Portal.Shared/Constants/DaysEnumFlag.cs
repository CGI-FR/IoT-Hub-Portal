// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Constants
{
    public static class DaysEnumFlag
    {
        [Flags]
        public enum DaysOfWeek
        {
            None = 0,
            Monday = 1 << 0,   // 1
            Tuesday = 1 << 1,  // 2
            Wednesday = 1 << 2,// 4
            Thursday = 1 << 3, // 8
            Friday = 1 << 4,   // 16
            Saturday = 1 << 5, // 32
            Sunday = 1 << 6    // 64
        }
    }
    public static class DayConverter
    {
        public static DaysEnumFlag.DaysOfWeek Convert(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => DaysEnumFlag.DaysOfWeek.Monday,
                DayOfWeek.Tuesday => DaysEnumFlag.DaysOfWeek.Tuesday,
                DayOfWeek.Wednesday => DaysEnumFlag.DaysOfWeek.Wednesday,
                DayOfWeek.Thursday => DaysEnumFlag.DaysOfWeek.Thursday,
                DayOfWeek.Friday => DaysEnumFlag.DaysOfWeek.Friday,
                DayOfWeek.Saturday => DaysEnumFlag.DaysOfWeek.Saturday,
                DayOfWeek.Sunday => DaysEnumFlag.DaysOfWeek.Sunday,
                _ => DaysEnumFlag.DaysOfWeek.None,
            };
        }
    }
}

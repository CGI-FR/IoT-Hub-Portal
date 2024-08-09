// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Constants
{
    using System;

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
        public static DaysEnumFlag.DaysOfWeek Convert(System.DayOfWeek day)
        {
            return day switch
            {
                System.DayOfWeek.Monday => DaysEnumFlag.DaysOfWeek.Monday,
                System.DayOfWeek.Tuesday => DaysEnumFlag.DaysOfWeek.Tuesday,
                System.DayOfWeek.Wednesday => DaysEnumFlag.DaysOfWeek.Wednesday,
                System.DayOfWeek.Thursday => DaysEnumFlag.DaysOfWeek.Thursday,
                System.DayOfWeek.Friday => DaysEnumFlag.DaysOfWeek.Friday,
                System.DayOfWeek.Saturday => DaysEnumFlag.DaysOfWeek.Saturday,
                System.DayOfWeek.Sunday => DaysEnumFlag.DaysOfWeek.Sunday,
                _ => DaysEnumFlag.DaysOfWeek.None,
            };
        }
    }
}

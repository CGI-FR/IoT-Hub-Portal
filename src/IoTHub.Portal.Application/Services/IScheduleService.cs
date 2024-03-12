// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IScheduleService
    {
        Task<ScheduleDto> CreateSchedule(ScheduleDto schedule);
        Task<Schedule> GetSchedule(string scheduleId);
    }
}

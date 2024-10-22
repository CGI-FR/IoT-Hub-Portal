// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{

    public class ScheduleProfile : Profile
    {
        public ScheduleProfile()
        {
            _ = CreateMap<ScheduleDto, Schedule>()
                .ReverseMap();
        }
    }
}

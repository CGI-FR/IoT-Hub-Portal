// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    public class DeviceTagProfile : Profile
    {
        public DeviceTagProfile()
        {
            _ = CreateMap<DeviceTagDto, DeviceTag>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Name))
                .ReverseMap();
        }
    }
}

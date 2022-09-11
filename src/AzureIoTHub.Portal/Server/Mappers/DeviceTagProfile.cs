// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AutoMapper;
    using Domain.Entities;

    public class DeviceTagProfile : Profile
    {
        public DeviceTagProfile()
        {
            _ = CreateMap<Models.v10.DeviceTag, DeviceTag>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Name))
                .ReverseMap();
        }
    }
}

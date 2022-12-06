// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Models.v10;

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

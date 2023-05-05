// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers.AWS
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;

    public class ThingTypeTagProfile : Profile
    {
        public ThingTypeTagProfile()
        {
            _ = CreateMap<ThingTypeTag, ThingTypeTagDto>()
                .ForMember(dest => dest.Key, opts => opts.MapFrom(src => src.Key))
                .ForMember(dest => dest.Key, opts => opts.MapFrom(src => src.Key))
                .ReverseMap();

        }
    }
}

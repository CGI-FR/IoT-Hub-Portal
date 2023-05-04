// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers.AWS
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;

    public class ThingTypeSearchableAttProfile : Profile
    {
        public ThingTypeSearchableAttProfile()
        {
            _ = CreateMap<ThingTypeSearchableAtt, ThingTypeSearchableAttDto>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ReverseMap();

        }
    }
}

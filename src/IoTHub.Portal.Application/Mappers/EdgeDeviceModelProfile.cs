// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Models.v10;

    public class EdgeDeviceModelProfile : Profile
    {
        public EdgeDeviceModelProfile()
        {
            _ = CreateMap<IoTEdgeModelDto, EdgeDeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ModelId))
                .ReverseMap();

            _ = CreateMap<IoTEdgeModelListItemDto, EdgeDeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ModelId))
                .ReverseMap();
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public class EdgeDeviceModelCommandProfile : Profile
    {
        public EdgeDeviceModelCommandProfile()
        {
            _ = CreateMap<IoTEdgeModuleCommandDto, EdgeDeviceModelCommand>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.CommandId))
                .ReverseMap();
        }
    }
}

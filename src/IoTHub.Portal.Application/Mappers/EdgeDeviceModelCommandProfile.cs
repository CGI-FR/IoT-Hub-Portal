// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using Shared.Models.v1._0;

    public class EdgeDeviceModelCommandProfile : Profile
    {
        public EdgeDeviceModelCommandProfile()
        {
            _ = CreateMap<IoTEdgeModuleCommand, EdgeDeviceModelCommand>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.CommandId))
                .ReverseMap();
        }
    }
}

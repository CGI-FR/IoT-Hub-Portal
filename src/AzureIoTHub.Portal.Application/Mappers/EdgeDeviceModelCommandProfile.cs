// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Shared.Models.v10;

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

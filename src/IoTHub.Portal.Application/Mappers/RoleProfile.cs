// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v1._0;

    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            _ = CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Avatar, opts => opts.MapFrom(src => src.Avatar));

            _ = CreateMap<RoleDto, Role>()
                 .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                 .ForMember(dest => dest.Avatar, opts => opts.MapFrom(src => src.Avatar));
        }
    }
}


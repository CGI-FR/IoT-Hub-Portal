// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v1._0;

    public class AccessControlProfile : Profile
    {
        public AccessControlProfile()
        {
            _ = CreateMap<AccessControl, AccessControlDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Scope, opts => opts.MapFrom(src => src.Scope))
                .ForMember(dest => dest.RoleId, opts => opts.MapFrom(src => src.RoleId));

            _ = CreateMap<AccessControlDto, AccessControl>()
                 .ForMember(dest => dest.Scope, opts => opts.MapFrom(src => src.Scope))
                 .ForMember(dest => dest.RoleId, opts => opts.MapFrom(src => src.RoleId));
        }
    }
}

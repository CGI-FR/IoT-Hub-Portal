// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public class AccessControlProfile : Profile
    {
        public AccessControlProfile()
        {
            _ = CreateMap<AccessControl, AccessControlModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.PrincipalId, opts => opts.MapFrom(src => src.PrincipalId))
                .ForMember(dest => dest.Scope, opts => opts.MapFrom(src => src.Scope))
                .ForMember(dest => dest.Role, opts => opts.MapFrom(src => src.Role != null
                    ? new RoleModel
                    {
                        Id = src.Role.Id,
                        Name = src.Role.Name
                    }
                    : null));

            _ = CreateMap<AccessControlModel, AccessControl>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PrincipalId, opts => opts.MapFrom(src => src.PrincipalId))
                .ForMember(dest => dest.Scope, opts => opts.MapFrom(src => src.Scope))
                .ForMember(dest => dest.RoleId, opts => opts.MapFrom(src => src.Role.Id))
                .ForMember(dest => dest.Role, opts => opts.Ignore());
        }
    }
}

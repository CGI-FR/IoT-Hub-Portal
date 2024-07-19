// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            _ = CreateMap<Role, RoleModel>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
               .ForMember(dest => dest.Color, opts => opts.MapFrom(src => src.Color));

            _ = CreateMap<RoleModel, Role>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Color, opts => opts.MapFrom(src => src.Color));

            _ = CreateMap<Role, RoleDetailsModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Color, opts => opts.MapFrom(src => src.Color))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.Actions, opts => opts.MapFrom(src =>
                    src.Actions.Select(a => a.Name)));

            _ = CreateMap<RoleDetailsModel, Role>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                 .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                 .ForMember(dest => dest.Color, opts => opts.MapFrom(src => src.Color))
                 .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                 .ForMember(dest => dest.Actions, opts => opts.MapFrom(src =>
                     src.Actions.Select(a => new Action { Name = a })));
        }
    }
}

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
               .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name));

            _ = CreateMap<Role, RoleDetailsModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Actions, opts => opts.MapFrom(src =>
                    src.Actions.Select(a => new ActionModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        RoleId = src.Id
                    })));

            _ = CreateMap<RoleModel, Role>()
                 .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name));
        }
    }
}

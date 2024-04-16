// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public class GroupProfile : Profile
    {
        public GroupProfile()
        {
            _ = CreateMap<Group, GroupModel>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name));

            _ = CreateMap<Group, GroupDetailsModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Users, opts => opts.MapFrom(src =>
                                   src.Members.Select(u => new UserModel
                                   {
                                       Id = u.User.Id,
                                       GivenName = u.User.GivenName
                                   })))
                .ForMember(dest => dest.AccessControls, opts => opts.MapFrom(src =>
                                   src.AccessControls.Select(ac => new AccessControlModel
                                   {
                                       Id = ac.Id,
                                       Scope = ac.Scope,
                                       Role = new RoleModel { Name = ac.Role.Name }
                                   })));

            _ = CreateMap<GroupModel, Group>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name));

            _ = CreateMap<GroupDetailsModel, Group>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AccessControls, opt => opt.MapFrom(src => src.AccessControls.Select(ac => new AccessControl
                {
                    Id = ac.Id,
                    Scope = ac.Scope,
                    Role = new Role { Name = ac.Role.Name }
                })));

        }
    }
}

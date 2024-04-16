// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public class UserProfile : Profile
    {
        public UserProfile()
        {
            _ = CreateMap<User, UserModel>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
               .ForMember(dest => dest.GivenName, opts => opts.MapFrom(src => src.GivenName));

            _ = CreateMap<User, UserDetailsModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opts => opts.MapFrom(src => src.Email))
                .ForMember(dest => dest.GivenName, opts => opts.MapFrom(src => src.GivenName))
                .ForMember(dest => dest.FamilyName, opts => opts.MapFrom(src => src.FamilyName))
                .ForMember(dest => dest.AccessControls, opts => opts.MapFrom(src =>
                               src.AccessControls.Select(ac => new AccessControlModel
                               {
                                   Id = ac.Id,
                                   Scope = ac.Scope,
                                   Role = new RoleModel { Name = ac.Role.Name }
                               })));

            _ = CreateMap<UserDetailsModel, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.GivenName, opt => opt.MapFrom(src => src.GivenName))
                .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.FamilyName))
                .ForMember(dest => dest.AccessControls, opt => opt.MapFrom(src => src.AccessControls.Select(ac => new AccessControl
                {
                    Id = ac.Id,
                    Scope = ac.Scope,
                    Role = new Role { Name = ac.Role.Name }
                })));
        }
    }
}

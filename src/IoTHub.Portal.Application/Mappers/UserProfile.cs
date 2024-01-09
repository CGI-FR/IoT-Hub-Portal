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
               .ForMember(dest => dest.Email, opts => opts.MapFrom(src => src.Email))
               .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
               .ForMember(dest => dest.Forename, opts => opts.MapFrom(src => src.FamilyName));

            _ = CreateMap<User, UserDetailsModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name));
            //  .ForMember(dest => dest.AccessControl, opts => opts.MapFrom(src =>
            //     src.AccessControls.Select(a => new ActionModel
            //   {
            //      Id = a.Id,
            //    Name = a.Name,
            //  UserId = src.Id
            //   })));
            _ = CreateMap<UserModel, User>()
                 .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name));
        }
    }
}

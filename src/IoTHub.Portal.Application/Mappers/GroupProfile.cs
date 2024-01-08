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
                                       Email = u.User.Email,
                                       Name = u.User.Name,
                                       Forename = u.User.Forename
                                   })));

            _ = CreateMap<GroupModel, Group>()
                 .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name));
        }
    }
}

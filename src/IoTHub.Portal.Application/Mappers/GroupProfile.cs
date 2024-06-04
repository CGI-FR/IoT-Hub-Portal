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
               .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
               .ForMember(dest => dest.Color, opts => opts.MapFrom(src => src.Color))
               .ForMember(dest => dest.PrincipalId, opts => opts.MapFrom(src => src.PrincipalId));

            _ = CreateMap<Group, GroupDetailsModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Color, opts => opts.MapFrom(src => src.Color))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.PrincipalId, opts => opts.MapFrom(src => src.PrincipalId));

            _ = CreateMap<GroupModel, Group>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Color, opts => opts.MapFrom(src => src.Color));

            _ = CreateMap<GroupDetailsModel, Group>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PrincipalId, opt => opt.Ignore())
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}

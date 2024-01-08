// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v1._0;

    public class GroupProfile : Profile
    {
        public GroupProfile()
        {
            _ = CreateMap<Group, GroupDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Avatar, opts => opts.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.AccessControls, opts => opts.MapFrom(src => src.GroupAccessControls.Select(ga => ga.AccessControlId)));

            _ = CreateMap<GroupDto, Group>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Avatar, opts => opts.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.GroupAccessControls, opts => opts.MapFrom(src =>
                    src.AccessControls.Select(id => new GroupAccessControl { AccessControlId = id })));
        }
    }
}


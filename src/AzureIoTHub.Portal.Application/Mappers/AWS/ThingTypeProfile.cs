// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers.AWS
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Shared.Models.v1._0.AWS;

    public class ThingTypeProfile : Profile
    {
        public ThingTypeProfile()
        {
            _ = CreateMap<ThingTypeDetails, ThingType>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ThingTypeID))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.ThingTypeName))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.ThingTypeDescription))
                .ForMember(dest => dest.ThingTypeSearchableAttributes, opts => opts.MapFrom(src => src.ThingTypeSearchableAttDtos.Select(pair => new ThingTypeSearchableAtt
                {
                    Name = pair.Name
                })))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.Select(pair => new ThingTypeTag
                {
                    Key = pair.Key,
                    Value = pair.Value
                })));

            _ = CreateMap<ThingType, ThingTypeDetails>()
                .ForMember(dest => dest.ThingTypeID, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.ThingTypeName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.ThingTypeDescription, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.ToDictionary(tag => tag.Key, tag => tag.Value)))
                .ForMember(dest => dest.ThingTypeSearchableAttDtos, opts => opts.MapFrom(src => src.ThingTypeSearchableAttributes.ToList()));

        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers.AWS
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using Amazon.IoT.Model;
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

            _ = CreateMap<ThingTypeDetails, CreateThingTypeRequest>()
                .ForMember(dest => dest.ThingTypeName, opts => opts.MapFrom(src => src.ThingTypeName))
                .ForMember(dest => dest.ThingTypeProperties, opts => opts.MapFrom(src => new ThingTypeProperties
                {
                    ThingTypeDescription = src.ThingTypeDescription,
                    SearchableAttributes = src.ThingTypeSearchableAttDtos.Select(pair => pair.Name).ToList() ?? new List<string>()
                }))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.Select(pair => new Tag
                {
                    Key = pair.Key,
                    Value = pair.Value
                }).ToList() ?? new List<Tag>()));

            _ = CreateMap<CreateThingTypeRequest, ThingTypeDetails>()
                .ForMember(dest => dest.ThingTypeName, opts => opts.MapFrom(src => src.ThingTypeName))
                .ForMember(dest => dest.ThingTypeDescription, opts => opts.MapFrom(src => src.ThingTypeProperties.ThingTypeDescription))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.ToDictionary(tag => tag.Key, tag => tag.Value)))
                .ForMember(dest => dest.ThingTypeSearchableAttDtos, opts => opts.MapFrom(src => src.ThingTypeProperties.SearchableAttributes.ToList()));

        }
    }
}

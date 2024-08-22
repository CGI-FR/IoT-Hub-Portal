// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using Amazon.IoT.Model;
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Shared;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;

    public class DeviceModelProfile : Profile
    {
        public DeviceModelProfile()
        {
            _ = CreateMap<DeviceModelDto, DeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ModelId))
                .ReverseMap();

            _ = CreateMap<LoRaDeviceModelDto, DeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ModelId))
                .ReverseMap();

            _ = CreateMap<DeviceModelDto, ExternalDeviceModelDto>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description));

            _ = CreateMap<ExternalDeviceModelDto, CreateThingTypeRequest>()
                .ForMember(dest => dest.ThingTypeName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.ThingTypeProperties, opts => opts.MapFrom(src => new ThingTypeProperties
                {
                    ThingTypeDescription = src.Description
                }));

            _ = CreateMap<DescribeThingTypeResponse, DeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ThingTypeId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.ThingTypeName))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.ThingTypeProperties.ThingTypeDescription ?? string.Empty));

            _ = CreateMap<DescribeThingResponse, ExternalDeviceModelDto>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.ThingTypeName));

            _ = CreateMap<DescribeThingTypeResponse, ExternalDeviceModelDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ThingTypeId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.ThingTypeName));
        }
    }
}

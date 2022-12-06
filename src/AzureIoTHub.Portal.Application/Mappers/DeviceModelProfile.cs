// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using Models.v10;
    using Models.v10.LoRaWAN;

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
        }
    }
}

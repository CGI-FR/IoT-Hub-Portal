// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using System.Linq;
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;
    using Models.v10;
    using Models.v10.LoRaWAN;

    public class DeviceProfile : Profile
    {
        public DeviceProfile()
        {
            _ = CreateMap<DeviceDetailsDto, Device>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceID))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.DeviceName))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.Select(pair => new DeviceTagValue
                {
                    Name = pair.Key,
                    Value = pair.Value
                })));

            _ = CreateMap<Device, DeviceDetailsDto>()
                .ForMember(dest => dest.DeviceID, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => src.DeviceModelId))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.ToDictionary(tag => tag.Name, tag => tag.Value)));

            _ = CreateMap<LorawanDevice, LorawanDevice>();

            _ = CreateMap<LoRaDeviceDetailsDto, LorawanDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceID))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.DeviceName))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.Select(pair => new DeviceTagValue
                {
                    Name = pair.Key,
                    Value = pair.Value
                })));

            _ = CreateMap<LorawanDevice, LoRaDeviceDetailsDto>()
                .ForMember(dest => dest.DeviceID, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => src.DeviceModelId))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.ToDictionary(tag => tag.Name, tag => tag.Value)));

            _ = CreateMap<LoRaTelemetry, LoRaTelemetryDto>();
            _ = CreateMap<LoRaDeviceTelemetry, LoRaDeviceTelemetryDto>();
        }
    }
}

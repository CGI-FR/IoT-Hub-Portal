// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using System.Linq;
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Models.v10;

    public class EdgeDeviceProfile : Profile
    {
        public EdgeDeviceProfile()
        {
            _ = CreateMap<EdgeDevice, EdgeDevice>();

            _ = CreateMap<EdgeDevice, IoTEdgeDevice>()
                .ForMember(dest => dest.DeviceId, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.ConnectionState, opts => opts.MapFrom(src => src.ConnectionState))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => src.DeviceModelId))
                .ForMember(dest => dest.Scope, opts => opts.MapFrom(src => src.Scope))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.IsEnabled))
                .ForMember(dest => dest.NbDevices, opts => opts.MapFrom(src => src.NbDevices))
                .ForMember(dest => dest.NbModules, opts => opts.MapFrom(src => src.NbModules))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.ToDictionary(tag => tag.Name, tag => tag.Value)));

            _ = CreateMap<IoTEdgeDevice, EdgeDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.DeviceName))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.Scope, opts => opts.MapFrom(src => src.Scope))
                .ForMember(dest => dest.ConnectionState, opts => opts.MapFrom(src => src.ConnectionState ?? "Disconnected"))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.IsEnabled))
                .ForMember(dest => dest.NbDevices, opts => opts.MapFrom(src => src.NbDevices))
                .ForMember(dest => dest.NbModules, opts => opts.MapFrom(src => src.NbModules))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.Select(pair => new DeviceTagValue
                {
                    Name = pair.Key,
                    Value = pair.Value
                })));

            _ = CreateMap<EdgeDevice, IoTEdgeListItem>()
                .ForMember(dest => dest.DeviceId, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.NbDevices, opts => opts.MapFrom(src => src.NbDevices))
                .ForMember(dest => dest.ImageUrl, opts => opts.MapFrom((src, _, _, context) => context.Items["imageUrl"]))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.ConnectionState))
                .ForMember(dest => dest.Labels, opts => opts.MapFrom(src => src.Labels.Union(src.DeviceModel.Labels)));
        }
    }
}

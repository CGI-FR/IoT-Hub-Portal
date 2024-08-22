// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using AutoMapper;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Domain.Entities;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json.Linq;
    using Shared.Models.v1._0;

    public class EdgeDeviceProfile : Profile
    {
        public EdgeDeviceProfile()
        {
            _ = CreateMap<EdgeDevice, EdgeDevice>();

            _ = CreateMap<Twin, EdgeDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Tags["deviceName"]))
                .ForMember(dest => dest.ConnectionState, opts => opts.MapFrom(src => src.ConnectionState.ToString()))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.Tags["modelId"]))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)))
                .ForMember(dest => dest.Scope, opts => opts.MapFrom(src => src.DeviceScope))
                .ForMember(dest => dest.NbDevices, opts => opts.MapFrom((src, _, _, context) => GetNbConnectedDevice((Twin)context.Items["TwinClient"])))
                .ForMember(dest => dest.NbModules, opts => opts.MapFrom((src, _, _, context) => DeviceHelper.RetrieveNbModuleCount((Twin)context.Items["TwinModules"], src.DeviceId)));

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

        private static ICollection<DeviceTagValue> GetTags(Twin twin)
        {
            return (JsonSerializer.Deserialize<Dictionary<string, object>>(twin.Tags.ToJson()) ?? new Dictionary<string, object>())
                .Where(tag => tag.Key is not "modelId" and not "deviceName")
                .Select(tag => new DeviceTagValue
                {
                    Name = tag.Key,
                    Value = tag.Value.ToString() ?? string.Empty
                })
                .ToList();
        }

        private static int GetNbConnectedDevice(Twin twin)
        {
            var reportedProperties = JObject.Parse(twin.Properties.Reported.ToJson());

            return reportedProperties.TryGetValue("clients", out var clients) ? clients.Count() : 0;
        }
    }
}

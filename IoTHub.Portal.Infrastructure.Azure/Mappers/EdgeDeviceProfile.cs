// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Azure.Helpers;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    public class EdgeDeviceProfile : Profile
    {
        public EdgeDeviceProfile()
        {
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

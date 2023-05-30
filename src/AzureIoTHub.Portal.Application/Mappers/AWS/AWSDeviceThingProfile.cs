// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers.AWS
{
    using System.Text;
    using Amazon.IoT.Model;
    using Amazon.IotData.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using Models.v10;
    using Newtonsoft.Json;

    public class AWSDeviceThingProfile : Profile
    {
        public AWSDeviceThingProfile()
        {
            _ = CreateMap<DeviceDetails, CreateThingRequest>()
                .ForMember(dest => dest.ThingName, opts => opts.MapFrom(src => src.DeviceName))
                .ForMember(dest => dest.ThingTypeName, opts => opts.MapFrom(src => src.ModelName))
                .ForPath(dest => dest.AttributePayload.Attributes, opts => opts.MapFrom(src => src.Tags))
                .ReverseMap();

            _ = CreateMap<DeviceDetails, UpdateThingRequest>()
                .ForMember(dest => dest.ThingName, opts => opts.MapFrom(src => src.DeviceName))
                .ForPath(dest => dest.AttributePayload.Attributes, opts => opts.MapFrom(src => src.Tags))
                .ReverseMap();

            _ = CreateMap<Device, DeleteThingRequest>()
                .ForMember(dest => dest.ThingName, opts => opts.MapFrom(src => src.Name))
                .ReverseMap();

            _ = CreateMap<DeviceDetails, UpdateThingShadowRequest>()
                .ForMember(dest => dest.ThingName, opts => opts.MapFrom(src => src.DeviceName))
                .ForMember(dest => dest.Payload, opts => opts.MapFrom(src => EmptyPayload()))
                .ReverseMap();

            _ = CreateMap<DescribeThingResponse, Device>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ThingId))
               .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.ThingName))
               .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
               .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Attributes.Select(att => new DeviceTagValue
               {
                   Name = att.Key,
                   Value = att.Value
               })));
        }

        private static MemoryStream EmptyPayload()
        {
            var json = new
            {
                state = new
                {
                    desired = new{},
                    reported = new{}
                }
            };

            return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json)));
        }
    }
}

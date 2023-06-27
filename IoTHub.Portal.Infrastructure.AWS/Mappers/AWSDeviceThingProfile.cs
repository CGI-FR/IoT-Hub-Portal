// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers.AWS
{
    using Amazon.IoT.Model;
    using Amazon.IotData.Model;
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Shared;
    using Models.v10;
    using Newtonsoft.Json;
    using System.Text;

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
               .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => true))
               .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Attributes.Select(att => new DeviceTagValue
               {
                   Name = att.Key,
                   Value = att.Value
               })));

            _ = CreateMap<DescribeThingResponse, EdgeDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ThingId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.ThingName))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => true))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Attributes.Select(att => new DeviceTagValue
                {
                    Name = att.Key,
                    Value = att.Value
                })));

            _ = CreateMap<IoTEdgeDevice, CreateThingRequest>()
                .ForMember(dest => dest.ThingName, opts => opts.MapFrom(src => src.DeviceName))
                .ForPath(dest => dest.AttributePayload.Attributes, opts => opts.MapFrom(src => src.Tags))
                .ReverseMap();

            _ = CreateMap<IoTEdgeDevice, UpdateThingRequest>()
                .ForMember(dest => dest.ThingName, opts => opts.MapFrom(src => src.DeviceName))
                .ForPath(dest => dest.AttributePayload.Attributes, opts => opts.MapFrom(src => src.Tags))
                .ReverseMap();

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

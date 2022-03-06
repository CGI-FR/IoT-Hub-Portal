// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Server.Entities;
    using AzureIoTHub.Portal.Shared.Models.V10;

    public class DevicePropertyProfile : Profile
    {
        public DevicePropertyProfile()
        {
            CreateMap<DeviceModelProperty, DeviceProperty>();

            CreateMap<DeviceProperty, DeviceModelProperty>()
                .ForMember(c => c.RowKey, opts => opts.MapFrom(c => c.Name))
                .ForMember(c => c.PartitionKey, opts => opts.MapFrom((src, dst, _, context) => context.Items[nameof(DeviceModelProperty.PartitionKey)]));
        }
    }
}

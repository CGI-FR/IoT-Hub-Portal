// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Server.Entities;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Extensions;

    public class DevicePropertyProfile : Profile
    {
        public DevicePropertyProfile()
        {
            _ = CreateMap<DeviceModelProperty, DeviceProperty>();

            _ = CreateMap<DeviceProperty, DeviceModelProperty>()
                .ForMember(c => c.RowKey, opts => opts.MapFrom(c => c.Name.KeepAuthorizedCharacters()))
                .ForMember(c => c.PartitionKey, opts => opts.MapFrom((_, _, _, context) => context.Items[nameof(DeviceModelProperty.PartitionKey)]));
        }
    }
}

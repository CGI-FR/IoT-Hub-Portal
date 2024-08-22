// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using Shared.Models.v1._0;

    public class DevicePropertyProfile : Profile
    {
        public DevicePropertyProfile()
        {
            _ = CreateMap<DeviceModelProperty, DeviceProperty>();

            _ = CreateMap<DeviceProperty, DeviceModelProperty>()
                .ForMember(c => c.ModelId, opts => opts.MapFrom((_, _, _, context) => context.Items[nameof(DeviceModelProperty.ModelId)]));
        }
    }
}

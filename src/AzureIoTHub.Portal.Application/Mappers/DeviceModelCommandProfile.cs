// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using Models.v10.LoRaWAN;

    public class DeviceModelCommandProfile : Profile
    {
        public DeviceModelCommandProfile()
        {
            _ = CreateMap<DeviceModelCommandDto, DeviceModelCommand>()
                .ForMember(c => c.Id, opts => opts.MapFrom(c => c.Id))
                .ReverseMap();
        }
    }
}

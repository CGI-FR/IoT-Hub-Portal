// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using Shared.Models.v1._0;

    public class LabelProfile : Profile
    {
        public LabelProfile()
        {
            _ = CreateMap<LabelDto, Label>()
                .ReverseMap();
        }
    }
}

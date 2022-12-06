// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Shared.Models.v10;

    public class LabelProfile : Profile
    {
        public LabelProfile()
        {
            _ = CreateMap<LabelDto, Label>()
                .ReverseMap();
        }
    }
}

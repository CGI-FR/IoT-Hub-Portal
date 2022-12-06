// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers
{
    using System.Collections.Generic;
    using AutoMapper;
    using Azure;

    public class EnumerableProfile : Profile
    {
        public EnumerableProfile()
        {
            _ = CreateMap(typeof(AsyncPageable<>), typeof(List<>));
        }
    }
}

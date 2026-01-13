// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{

    public class LabelProfile : Profile
    {
        public LabelProfile()
        {
            _ = CreateMap<LabelDto, Label>()
                .ReverseMap();
        }
    }
}

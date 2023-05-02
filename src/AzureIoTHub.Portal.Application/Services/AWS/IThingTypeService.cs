// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services.AWS
{
    using AzureIoTHub.Portal.Models.v10.AWS;

    public interface IThingTypeService
    {
        //Create a thing type
        Task<ThingTypeDto> CreateThingType(ThingTypeDto thingType);
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services.AWS
{
    using AzureIoTHub.Portal.Models.v10.AWS;
    using Microsoft.AspNetCore.Http;

    public interface IThingTypeService
    {
        //Create a thing type
        Task<string> CreateThingType(ThingTypeDto thingType);

        Task<string> GetThingTypeAvatar(string thingTypeId);

        Task<string> UpdateThingTypeAvatar(string thingTypeId, IFormFile file);

        Task DeleteThingTypeAvatar(string thingTypeId);
    }
}

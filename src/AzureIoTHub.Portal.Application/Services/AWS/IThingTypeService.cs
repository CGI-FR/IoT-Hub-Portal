// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services.AWS
{
    using AzureIoTHub.Portal.Models.v10.AWS;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Http;

    public interface IThingTypeService
    {
        //Get All Thing Types
        Task<PaginatedResult<ThingTypeDto>> GetThingTypes(DeviceModelFilter deviceModelFilter);

        //Get a thing type
        Task<ThingTypeDto> GetThingType(string thingTypeId);
        //Create a thing type
        Task<string> CreateThingType(ThingTypeDto thingType);
        Task<ThingTypeDto> DeprecateThingType(string thingTypeId);


        Task<string> GetThingTypeAvatar(string thingTypeId);

        Task<string> UpdateThingTypeAvatar(string thingTypeId, IFormFile file);

        Task DeleteThingTypeAvatar(string thingTypeId);
    }
}

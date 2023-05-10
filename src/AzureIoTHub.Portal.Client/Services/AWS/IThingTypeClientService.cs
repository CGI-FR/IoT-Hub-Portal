// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services.AWS
{
    using AzureIoTHub.Portal.Models.v10.AWS;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;

    public interface IThingTypeClientService
    {
        Task<PaginationResult<ThingTypeDto>> GetThingTypes(DeviceModelFilter? deviceModelFilter = null);
        Task<ThingTypeDto> GetThingType(string thingTypeId);

        Task<string> CreateThingType(ThingTypeDto thingType);
        Task DeprecateThingType(string thingTypeId);
        Task DeleteThingType(string thingTypeId);
        Task<string> GetAvatarUrl(string thingTypeId);

        Task ChangeAvatar(string thingTypeId, MultipartFormDataContent avatar);

    }
}

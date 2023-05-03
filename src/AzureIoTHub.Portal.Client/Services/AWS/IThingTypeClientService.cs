// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services.AWS
{
    using AzureIoTHub.Portal.Models.v10.AWS;

    public interface IThingTypeClientService
    {
        Task<string> CreateThingType(ThingTypeDto thingType);

        Task<string> GetAvatarUrl(string thingTypeId);

        Task ChangeAvatar(string thingTypeId, MultipartFormDataContent avatar);

    }
}

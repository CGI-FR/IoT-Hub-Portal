// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services.AWS
{
    using AzureIoTHub.Portal.Models.v10.AWS;

    public interface IThingTypeClientService
    {
        Task CreateThingType(ThingTypeDto thingType);

    }
}

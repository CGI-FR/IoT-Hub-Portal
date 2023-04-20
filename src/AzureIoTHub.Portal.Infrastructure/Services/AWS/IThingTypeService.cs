// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using AzureIoTHub.Portal.Shared.Models.v1._0.AWS;

    public interface IThingTypeService<TDto>
        where TDto : IThingTypeDetails
    {
        //Create a thing type
        Task<TDto> CreateThingType(TDto thingType);
    }
}

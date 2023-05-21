// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Shared;

    public interface IExternalDeviceServiceV2
    {
        Task<ExternalDeviceModelDto> CreateDeviceModel(ExternalDeviceModelDto deviceModel);

        Task DeleteDeviceModel(ExternalDeviceModelDto deviceModel);
    }
}

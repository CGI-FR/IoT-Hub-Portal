// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Shared.Models;

    public interface IDeviceModelMapper
    {
        DeviceModel CreateDeviceModel(TableEntity entity);

        void UpdateTableEntity(TableEntity entity, DeviceModel model);
    }
}

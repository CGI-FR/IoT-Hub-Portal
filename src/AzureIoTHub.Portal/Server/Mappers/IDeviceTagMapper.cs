// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;

    public interface IDeviceTagMapper
    {
        public DeviceTag GetDeviceTag(TableEntity entity);
        public void UpdateTableEntity(TableEntity tagEntity, DeviceTag element);
    }
}

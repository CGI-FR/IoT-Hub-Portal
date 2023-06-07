// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    using Azure.Data.Tables;
    using IoTHub.Portal.Models.v10;

    public interface IDeviceTagMapper
    {
        public DeviceTagDto GetDeviceTag(TableEntity entity);
        public void UpdateTableEntity(TableEntity tagEntity, DeviceTagDto element);
    }
}

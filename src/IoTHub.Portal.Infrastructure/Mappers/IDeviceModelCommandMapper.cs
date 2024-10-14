// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    public interface IDeviceModelCommandMapper
    {
        public DeviceModelCommandDto GetDeviceModelCommand(TableEntity entity);

        public void UpdateTableEntity(TableEntity commandEntity, DeviceModelCommandDto element);
    }
}

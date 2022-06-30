// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System.Collections.Generic;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;

    public interface IEdgeDeviceModelMapper
    {
        IoTEdgeModelListItem CreateEdgeDeviceModelListItem(TableEntity entity);

        IoTEdgeModel CreateEdgeDeviceModel(TableEntity entity, List<IoTEdgeModule> ioTEdgeModules);

        Dictionary<string, object> UpdateTableEntity(TableEntity entity, IoTEdgeModel model);
    }
}

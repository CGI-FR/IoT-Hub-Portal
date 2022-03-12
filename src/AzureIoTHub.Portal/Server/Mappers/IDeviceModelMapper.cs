// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Shared.Models.v10.DeviceModel;
    using System.Collections.Generic;

    public interface IDeviceModelMapper<TListItem, TModel>
        where TModel : DeviceModel
        where TListItem : DeviceModel
    {
        public TListItem CreateDeviceModelListItem(TableEntity entity);

        public TModel CreateDeviceModel(TableEntity entity);

        public Dictionary<string, object> UpdateTableEntity(TableEntity entity, TModel model);
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;
    using System.Collections.Generic;

    public interface IDeviceModelMapper<TListItem, TModel>
        where TListItem : DeviceModel
        where TModel : DeviceModel
    {
        public TListItem CreateDeviceModelListItem(TableEntity entity);

        public TModel CreateDeviceModel(TableEntity entity);

        public Dictionary<string, object> UpdateTableEntity(TableEntity entity, TModel model);
    }
}

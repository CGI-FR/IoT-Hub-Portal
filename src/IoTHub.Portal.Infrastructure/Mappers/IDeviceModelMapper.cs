// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    using Azure.Data.Tables;
    using IoTHub.Portal.Shared.Models;
    using System.Collections.Generic;
    using Shared.Models.v1._0;

    public interface IDeviceModelMapper<TListItem, TModel>
        where TListItem : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
        public TListItem CreateDeviceModelListItem(TableEntity entity);

        public TModel CreateDeviceModel(TableEntity entity);

        public Dictionary<string, object> BuildDeviceModelDesiredProperties(TModel model);
    }
}

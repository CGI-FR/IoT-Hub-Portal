// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    public interface IDeviceModelMapper<TListItem, TModel>
        where TListItem : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
        TListItem CreateDeviceModelListItem(TableEntity entity);

        TModel CreateDeviceModel(TableEntity entity);

        Dictionary<string, object> BuildDeviceModelDesiredProperties(TModel model);
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Managers;

    public class EdgeDeviceModelMapper : IEdgeDeviceModelMapper
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public EdgeDeviceModelMapper(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public IoTEdgeModelListItem CreateEdgeDeviceModelListItem(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new IoTEdgeModelListItem
            {
                ModelId = entity.RowKey,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(IoTEdgeModelListItem.Name)]?.ToString(),
                Description = entity[nameof(IoTEdgeModelListItem.Description)]?.ToString(),
            };
        }

        public IoTEdgeModel CreateEdgeDeviceModel(TableEntity entity, List<IoTEdgeModule> ioTEdgeModules)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new IoTEdgeModel
            {
                ModelId = entity.RowKey,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(IoTEdgeModelListItem.Name)]?.ToString(),
                Description = entity[nameof(IoTEdgeModelListItem.Description)]?.ToString(),
                EdgeModules = ioTEdgeModules
            };
        }

        public Dictionary<string, IoTEdgeModule> UpdateTableEntity(TableEntity entity, IoTEdgeModel model)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            entity[nameof(IoTEdgeModel.Name)] = model.Name;
            entity[nameof(IoTEdgeModel.Description)] = model.Description;

            var EdgeModuleList = new Dictionary<string, IoTEdgeModule>();

            foreach (var module in model.EdgeModules)
            {
                AddEdgeModule(module.ModuleName, module, EdgeModuleList);
            }

            return EdgeModuleList;
        }

        private static void AddEdgeModule(string moduleName, IoTEdgeModule module, Dictionary<string, IoTEdgeModule> moduleList)
        {
            moduleList.Add(moduleName, module);
        }
    }
}

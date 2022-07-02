// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Factories
{
    using Azure.Data.Tables;

    public interface ITableClientFactory
    {
        const string DeviceCommandTableName = "DeviceCommands";
        const string DeviceTemplateTableName = "DeviceTemplates";
        const string EdgeDeviceTemplateTableName = "EdgeDeviceTemplates";
        const string DeviceTagSettingTableName = "DeviceTagSettings";
        const string DeviceTemplatePropertiesTableName = "DeviceTemplateProperties";

        TableClient GetDeviceCommands();

        TableClient GetDeviceTemplates();

        TableClient GetEdgeDeviceTemplates();

        TableClient GetDeviceTemplateProperties();

        TableClient GetDeviceTagSettings();

        public TableClient GetTemplatesHealthCheck();
    }
}

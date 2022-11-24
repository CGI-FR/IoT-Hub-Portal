// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Factories
{
    using Azure.Data.Tables;

    public class TableClientFactory : ITableClientFactory
    {
        private readonly string connectionString;

        internal const string DeviceCommandTableName = "DeviceCommands";
        internal const string DeviceTemplateTableName = "DeviceTemplates";
        internal const string EdgeDeviceTemplateTableName = "EdgeDeviceTemplates";
        internal const string DeviceTagSettingTableName = "DeviceTagSettings";
        internal const string DeviceTemplatePropertiesTableName = "DeviceTemplateProperties";
        internal const string EdgeModuleCommandsTableName = "EdgeModuleCommands";

        public TableClientFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public TableClient GetDeviceCommands()
        {
            return CreateClient(DeviceCommandTableName);
        }

        public TableClient GetDeviceTemplates()
        {
            return CreateClient(DeviceTemplateTableName);
        }

        public TableClient GetEdgeDeviceTemplates()
        {
            return CreateClient(EdgeDeviceTemplateTableName);
        }

        public TableClient GetDeviceTagSettings()
        {
            return CreateClient(DeviceTagSettingTableName);
        }

        private TableClient CreateClient(string tableName)
        {
            var tableClient = new TableClient(this.connectionString, tableName);
            _ = tableClient.CreateIfNotExists();

            return tableClient;
        }

        public TableClient GetDeviceTemplateProperties()
        {
            return CreateClient(DeviceTemplatePropertiesTableName);
        }

        public TableClient GetTemplatesHealthCheck()
        {
            return CreateClient("tableHealthCheck");
        }

        public TableClient GetEdgeModuleCommands()
        {
            return CreateClient(EdgeModuleCommandsTableName);
        }
    }
}

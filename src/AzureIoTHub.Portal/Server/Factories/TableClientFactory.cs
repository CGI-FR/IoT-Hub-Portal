// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Factories
{
    using Azure.Data.Tables;

    public class TableClientFactory : ITableClientFactory
    {
        private readonly string connectionString;

        public TableClientFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public TableClient GetDeviceCommands()
        {
            return this.CreateClient(ITableClientFactory.DeviceCommandTableName);
        }

        public TableClient GetDeviceTemplates()
        {
            return this.CreateClient(ITableClientFactory.DeviceTemplateTableName);
        }

        public TableClient GetDeviceTagSettings()
        {
            return this.CreateClient(ITableClientFactory.DeviceTagSettingTableName);
        }

        private TableClient CreateClient(string tableName)
        {
            var tableClient = new TableClient(this.connectionString, tableName);

            tableClient.CreateIfNotExists();

            return tableClient;
        }

        public TableClient GetDeviceTemplateProperties()
        {
            return this.CreateClient(ITableClientFactory.DeviceTemplatePropertiesTableName);
        }
    }
}

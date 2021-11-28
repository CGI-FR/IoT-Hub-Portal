// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Factories
{
    using System.Threading.Tasks;
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

        private TableClient CreateClient(string tableName)
        {
            var tableClient = new TableClient(this.connectionString, tableName);

            tableClient.CreateIfNotExists();

            return tableClient;
        }
    }
}

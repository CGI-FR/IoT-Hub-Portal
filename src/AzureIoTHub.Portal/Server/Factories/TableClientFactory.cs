// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Factories
{
    using System;
    using Azure;
    using Azure.Data.Tables;
    using Exceptions;

    public class TableClientFactory : ITableClientFactory
    {
        private readonly string connectionString;

        public TableClientFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public TableClient GetDeviceCommands()
        {
            return CreateClient(ITableClientFactory.DeviceCommandTableName);
        }

        public TableClient GetDeviceTemplates()
        {
            return CreateClient(ITableClientFactory.DeviceTemplateTableName);
        }

        public TableClient GetDeviceTagSettings()
        {
            return CreateClient(ITableClientFactory.DeviceTagSettingTableName);
        }

        private TableClient CreateClient(string tableName)
        {
            try
            {
                var tableClient = new TableClient(this.connectionString, tableName);

                _ = tableClient.CreateIfNotExists();

                return tableClient;
            }
            catch (Exception e) when (e is ArgumentNullException or
                                          InvalidOperationException or
                                          RequestFailedException)
            {
                throw new InternalServerErrorException($"Unable to create table client with table name {tableName}", e);
            }
        }

        public TableClient GetDeviceTemplateProperties()
        {
            return CreateClient(ITableClientFactory.DeviceTemplatePropertiesTableName);
        }

        public TableClient GetTemplatesHealthCheck()
        {
            return CreateClient("tableHealthCheck");
        }
    }
}

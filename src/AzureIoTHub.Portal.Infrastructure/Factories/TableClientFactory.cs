// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Factories
{
    using System;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;

    public class TableClientFactory : ITableClientFactory
    {
        private readonly string connectionString;

        private const string DeviceCommandTableName = "DeviceCommands";
        private const string DeviceTemplateTableName = "DeviceTemplates";
        private const string EdgeDeviceTemplateTableName = "EdgeDeviceTemplates";
        private const string DeviceTagSettingTableName = "DeviceTagSettings";
        private const string DeviceTemplatePropertiesTableName = "DeviceTemplateProperties";
        private const string EdgeModuleCommandsTableName = "EdgeModuleCommands";

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

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Factories
{
    using Azure.Data.Tables;

    public interface ITableClientFactory
    {
        const string DeviceCommandTableName = "DeviceCommands";
        const string DeviceTemplateTableName = "DeviceTemplates";

        TableClient GetDeviceCommands();

        TableClient GetDeviceTemplates();
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Factories
{
    using Azure.Data.Tables;

    public interface ITableClientFactory
    {
        TableClient GetDeviceCommands();

        TableClient GetDeviceTemplates();

        TableClient GetEdgeDeviceTemplates();

        TableClient GetDeviceTemplateProperties();

        TableClient GetDeviceTagSettings();

        TableClient GetTemplatesHealthCheck();

        TableClient GetEdgeModuleCommands();
    }
}

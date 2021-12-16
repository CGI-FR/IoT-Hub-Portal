// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Collections.Generic;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Models.Device;

    public class DeviceModelCommandsManager : IDeviceModelCommandsManager
    {
        private readonly ITableClientFactory tableClientFactory;

        public DeviceModelCommandsManager(ITableClientFactory tableClientFactory)
        {
            this.tableClientFactory = tableClientFactory;
        }

        /// <summary>
        /// Retrieve all the commands from a devicemodel.
        /// </summary>
        /// <param name="deviceModel"> the model type of the device.</param>
        /// <returns>Corresponding list of commands or an empty list if it doesn't have any command.</returns>
        public List<Command> RetrieveCommands(string deviceModel)
        {
            var commands = new List<Command>();

            if (deviceModel == null)
            {
                return commands;
            }

            var queryResultsFilter = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"PartitionKey  eq '{deviceModel}'");

            foreach (TableEntity qEntity in queryResultsFilter)
            {
                commands.Add(
                    new Command()
                    {
                        CommandId = qEntity.RowKey,
                        Frame = qEntity[nameof(Command.Frame)].ToString()
                    });
            }

            return commands;
        }

        public List<DeviceModelCommand> RetrieveDeviceModelCommands(string deviceModel)
        {
            var commands = new List<DeviceModelCommand>();

            if (deviceModel == null)
            {
                return commands;
            }

            var queryResultsFilter = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"PartitionKey  eq '{deviceModel}'");

            foreach (TableEntity qEntity in queryResultsFilter)
            {
                try
                {
                    commands.Add(
                    new DeviceModelCommand()
                    {
                        CommandId = qEntity.RowKey,
                        Name = qEntity.RowKey,
                        Frame = qEntity[nameof(DeviceModelCommand.Frame)].ToString(),
                        Port = int.Parse(qEntity[nameof(DeviceModelCommand.Port)].ToString())
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return commands;
        }
    }
}

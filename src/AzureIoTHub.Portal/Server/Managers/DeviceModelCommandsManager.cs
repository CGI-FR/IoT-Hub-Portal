// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public class DeviceModelCommandsManager : IDeviceModelCommandsManager
    {
        private readonly ITableClientFactory tableClientFactory;
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;

        public DeviceModelCommandsManager(ITableClientFactory tableClientFactory, IDeviceModelCommandMapper deviceModelCommandMapper)
        {
            this.tableClientFactory = tableClientFactory;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
        }

        /// <summary>
        /// Retrieve all the commands from a devicemodel.
        /// </summary>
        /// <param name="deviceModel"> the model type of the device.</param>
        /// <returns>Corresponding list of commands or an empty list if it doesn't have any command.</returns>
        public ReadOnlyCollection<Command> RetrieveCommands(string deviceModel)
        {
            var commands = new List<Command>();

            if (deviceModel == null)
            {
                return commands.AsReadOnly();
            }

            var queryResultsFilter = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"PartitionKey  eq '{deviceModel}'");

            foreach (var qEntity in queryResultsFilter)
            {
                commands.Add(
                    new Command()
                    {
                        CommandId = qEntity.RowKey,
                        Frame = qEntity[nameof(Command.Frame)].ToString()
                    });
            }

            return commands.AsReadOnly();
        }

        public ReadOnlyCollection<DeviceModelCommand> RetrieveDeviceModelCommands(string deviceModel)
        {
            var commands = new List<DeviceModelCommand>();

            if (deviceModel == null)
            {
                return commands.AsReadOnly();
            }

            var queryResultsFilter = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"PartitionKey  eq '{deviceModel}'");

            commands.AddRange(queryResultsFilter.Select(this.deviceModelCommandMapper.GetDeviceModelCommand));

            return commands.AsReadOnly();
        }
    }
}

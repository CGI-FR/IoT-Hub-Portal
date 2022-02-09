// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Collections.Generic;
    using System.Linq;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;

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

            commands.AddRange(queryResultsFilter.Select(this.deviceModelCommandMapper.GetDeviceModelCommand));

            return commands;
        }
    }
}

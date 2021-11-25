// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Helpers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Shared.Models;

    public static class SensorsHelper
    {
        /// <summary>
        /// Creates a SensorModel object from a query result.
        /// Checks first if the entity fields fit to the sensor model attributes.
        /// </summary>
        /// <param name="entity">An AzureDataTable entity coming from a query.</param>
        /// <returns>A sensor model.</returns>
        public static SensorModel MapTableEntityToSensorModel(TableEntity entity)
        {
            return new SensorModel()
            {
                Name = entity.RowKey,
                Description = RetrieveValue(entity, "Description"),
                AppEUI = RetrieveValue(entity, "AppEUI")
            };
        }

        /// <summary>
        /// Creates a Sensor command Model object from a query result.
        /// Checks first if the entity fields fit to the sensor model attributes.
        /// </summary>
        /// <param name="entity">An AzureDataTable entity coming from a query.</param>
        /// <returns>A sensor command model.</returns>
        public static SensorCommand MapTableEntityToSensorCommand(TableEntity entity)
        {
            return new SensorCommand()
            {
                Name = entity.RowKey,
                Trame = RetrieveValue(entity, "Trame"),
                Port = int.Parse(RetrieveValue(entity, "Port"))
            };
        }

        public static string RetrieveValue(TableEntity entity, string name)
        {
            if (entity.ContainsKey(name))
                return entity[name].ToString();
            else
                return null;
        }
    }
}

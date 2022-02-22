using Azure.Data.Tables;
using AzureIoTHub.Portal.Shared.Models.v10.Device;

namespace AzureIoTHub.Portal.Server.Mappers
{
    public class DeviceTagMapper : IDeviceTagMapper
    {
        
        /// <summary>
        /// Gets a device tag setting.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A device tag setting</returns>
        public DeviceTag GetDeviceTag(TableEntity entity)
        {
            return new DeviceTag
            {
                Name = entity.RowKey,
                Label = entity[nameof(DeviceTag.Label)].ToString(),
                Required = bool.Parse(entity[nameof(DeviceTag.Required)].ToString() ?? "false"),
                Searchable = bool.Parse(entity[nameof(DeviceTag.Searchable)].ToString() ?? "false")
            };
        }

        /// <summary>
        /// Updates the table entity.
        /// </summary>
        /// <param name="tagEntity">The entity.</param>
        /// <param name="element">The device tag object.</param>
        public void UpdateTableEntity(TableEntity tagEntity, DeviceTag element)
        {
            tagEntity[nameof(DeviceTag.Label)] = element.Label;
            tagEntity[nameof(DeviceTag.Required)] = element.Required;
            tagEntity[nameof(DeviceTag.Searchable)] = element.Searchable;
        }
    }
}

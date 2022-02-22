using Azure.Data.Tables;
using AzureIoTHub.Portal.Shared.Models.v10.Device;

namespace AzureIoTHub.Portal.Server.Mappers
{
    public interface IDeviceTagMapper
    {
        public DeviceTag GetDeviceTag(TableEntity entity);
        public void UpdateTableEntity(TableEntity tagEntity, DeviceTag element);
    }
}

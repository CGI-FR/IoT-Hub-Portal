using AzureIoTHub.Portal.Shared.Models.V10.Device;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Services
{
    public interface IDeviceTagService
    {
        IEnumerable<DeviceTag> GetAllTags();

        IEnumerable<string> GetAllTagsNames();

        IEnumerable<string> GetAllSearchableTagsNames();

        Task UpdateTags(List<DeviceTag> tags);
    }
}

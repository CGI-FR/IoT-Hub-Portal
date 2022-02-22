using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Shared.Models.v10.Device
{
    public class DeviceTag
    {
        /// <summary>
        /// The registered name in the device twin.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The label shown to the user.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Whether the field is required when creating a new device or not.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Whether the field can be searcheable via the device search panel or not.
        /// </summary>
        public bool Searchable { get; set; }
    }
}

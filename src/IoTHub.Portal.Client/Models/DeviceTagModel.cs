// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Models
{
    using Portal.Shared.Models.v1._0;

    public class DeviceTagModel : DeviceTagDto
    {
        public DeviceTagModel()
        {
            IsNewTag = true;
        }

        public DeviceTagModel(DeviceTagDto deviceTag)
        {
            Name = deviceTag.Name;
            Label = deviceTag.Label;
            Required = deviceTag.Required;
            Searchable = deviceTag.Searchable;
        }

        public bool IsNewTag { get; set; }
    }
}

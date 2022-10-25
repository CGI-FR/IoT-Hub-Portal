// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Models
{
    using Portal.Models.v10;

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

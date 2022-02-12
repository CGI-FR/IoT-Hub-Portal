// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;

    public class ConfigItem
    {
        public string Name { get; set; }

        public DateTime DateCreation { get; set; }

        public string Status { get; set; }

        public ConfigItem()
        {
            this.DateCreation = new DateTime(1999, 1, 1);
        }
    }
}

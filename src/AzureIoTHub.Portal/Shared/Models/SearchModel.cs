// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SearchModel
    {
        public string DeviceId { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        public SearchModel(string id = null, string status = null, string type = null)
        {
            this.DeviceId = id;
            this.Status = status;
            this.Type = type;
        }
    }
}

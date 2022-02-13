// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
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

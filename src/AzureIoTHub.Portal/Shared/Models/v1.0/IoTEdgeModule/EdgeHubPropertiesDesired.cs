// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.IoTEdgeModule
{
    using System.Collections.Generic;

    public class EdgeHubPropertiesDesired
    {
        public IDictionary<string, object> Routes { get; set; }

        public string SchemaVersion { get; set; }

        public StoreAndForwardConfiguration StoreAndForwardConfiguration { get; set; }

        public EdgeHubPropertiesDesired()
        {
            this.Routes = new Dictionary<string, object>();
            this.SchemaVersion = "1.1";
            this.StoreAndForwardConfiguration = new StoreAndForwardConfiguration();
        }
    }

    public class StoreAndForwardConfiguration
    {
        public int TimeToLiveSecs { get; set; }

        public StoreAndForwardConfiguration()
        {
            this.TimeToLiveSecs = 7200;
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10.IoTEdgeModule
{
    public class EdgeHubPropertiesDesired
    {
        [JsonPropertyName("routes")]
        [JsonProperty(PropertyName = "routes")]
        public IDictionary<string, object> Routes { get; set; }

        [JsonPropertyName("schemaVersion")]
        [JsonProperty(PropertyName = "schemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonPropertyName("storeAndForwardConfiguration")]
        [JsonProperty(PropertyName = "storeAndForwardConfiguration")]
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
        [JsonPropertyName("timeToLiveSecs")]
        [JsonProperty(PropertyName = "timeToLiveSecs")]
        public int TimeToLiveSecs { get; set; }

        public StoreAndForwardConfiguration()
        {
            this.TimeToLiveSecs = 7200;
        }
    }
}

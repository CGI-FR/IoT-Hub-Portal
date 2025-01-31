// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    public class SubmitIdeaRequest
    {
        [JsonPropertyName("organization")]
        [JsonProperty("organization")]
        public string Organization => "CGI-FR";

        [JsonPropertyName("repository")]
        [JsonProperty("repository")]
        public string Repository => "IoT-Hub-Portal";

        [JsonPropertyName("title")]
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services.AWS
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.AWS;

    public class ThingTypeClientService : IThingTypeClientService
    {
        private readonly HttpClient http;

        public ThingTypeClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task CreateThingType(ThingTypeDto thingType)
        {
            return this.http.PostAsJsonAsync("api/aws/thingtypes", thingType);
        }
    }
}

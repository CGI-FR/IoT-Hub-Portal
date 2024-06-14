// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Shared.Models.v10;

    public class IdeaClientService : IIdeaClientService
    {
        private readonly HttpClient http;

        public IdeaClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IdeaResponse> SubmitIdea(IdeaRequest ideaRequest)
        {
            var response = await this.http.PostAsJsonAsync("api/ideas", ideaRequest);

            return await response.Content.ReadFromJsonAsync<IdeaResponse>() ?? new IdeaResponse();
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
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

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Shared.Models.v1._0;

    public class IdeaService : IIdeaService
    {
        private readonly ILogger<IdeaService> logger;
        private readonly HttpClient http;
        private readonly ConfigHandler configHandler;

        public IdeaService(ILogger<IdeaService> logger, HttpClient httpClient, ConfigHandler configHandler)
        {
            this.logger = logger;
            this.http = httpClient;
            this.configHandler = configHandler;
        }

        public async Task<IdeaResponse> SubmitIdea(IdeaRequest ideaRequest)
        {
            if (!this.configHandler.IdeasEnabled)
            {
                throw new InternalServerErrorException("Ideas feature is not enabled. Please check Iot Hub Portal documentation");
            }

            var submitIdea = new SubmitIdeaRequest
            {
                Title = ideaRequest.Title,
                Description = ideaRequest.Body
            };

            var ideaAsJson = JsonConvert.SerializeObject(submitIdea);

            this.logger.LogInformation($"Begin submitting a user idea: {ideaAsJson}");

            using var content = new StringContent(ideaAsJson, Encoding.UTF8, "application/json");

            var response = await this.http.PostAsync("ideas", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadFromJsonAsync<IdeaResponse>();

                this.logger.LogInformation($"User idea has been successfully submitted: {responseBody?.Url}");

                return responseBody;
            }

            this.logger.LogError($"Unable to submit user idea. (Status: {response.StatusCode}, ErrorBody: {await response.Content.ReadAsStringAsync()})");

            throw new InternalServerErrorException($"Unable to submit your idea. Reason: {response.ReasonPhrase}");
        }
    }
}

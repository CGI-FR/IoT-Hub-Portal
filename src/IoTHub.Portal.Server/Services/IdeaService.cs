// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace IoTHub.Portal.Server.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Shared.Models.v10;
    using UAParser;

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

        public async Task<IdeaResponse> SubmitIdea(IdeaRequest ideaRequest, string? userAgent = null)
        {
            if (!this.configHandler.IdeasEnabled)
            {
                throw new InternalServerErrorException("Ideas feature is not enabled. Please check Iot Hub Portal documentation");
            }

            var uaParser = Parser.GetDefault();

            var c = uaParser.Parse(userAgent);

            var submitIdea = new SubmitIdeaRequest();

            if (ideaRequest.ConsentToCollectTechnicalDetails)
            {
                var description = new StringBuilder();
                _ = description.Append("Description: ");
                _ = description.Append(ideaRequest.Body);
                _ = description.AppendLine();
                //_ = description.Append("Subscription ID: ");
                //_ = description.Append(configHandler.IdeasAuthenticationToken);
                //_ = description.AppendLine();
                _ = description.Append("Application Version: ");
                _ = description.Append(Assembly.GetExecutingAssembly().GetName().Version);
                _ = description.AppendLine();
                _ = description.Append("Browser Version: ");
                _ = description.Append(string.Concat(c.UA.Family, c.UA.Major, c.UA.Minor));

                submitIdea.Title = ideaRequest.Title;
                submitIdea.Description = description.ToString();
            }
            else
            {
                submitIdea.Title = ideaRequest.Title;
                submitIdea.Description = ideaRequest.Body;
            }

            var ideaAsJson = JsonConvert.SerializeObject(submitIdea);

            this.logger.LogInformation($"Begin submitting a user idea: {ideaAsJson}");

            using var content = new StringContent(ideaAsJson, Encoding.UTF8, "application/json");

            var response = await this.http.PostAsync("ideas", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadFromJsonAsync<IdeaResponse>();

                this.logger.LogInformation($"User idea has been successfully submitted: {responseBody?.Url}");

                return responseBody!;
            }

            this.logger.LogError($"Unable to submit user idea. (Status: {response.StatusCode}, ErrorBody: {await response.Content.ReadAsStringAsync()})");

            throw new InternalServerErrorException($"Unable to submit your idea. Reason: {response.ReasonPhrase}");
        }
    }
}

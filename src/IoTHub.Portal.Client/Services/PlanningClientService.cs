// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public class PlanningClientService : IPlanningClientService
    {
        private readonly HttpClient http;

        public PlanningClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreatePlanning(PlanningDto planning)
        {
            var response = await this.http.PostAsJsonAsync("api/planning", planning);

            //Retrieve Device ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedPlanning = Newtonsoft.Json.JsonConvert.DeserializeObject<PlanningDto>(responseJson);

            return updatedPlanning.Id.ToString();
        }

        public Task<PlanningDto> GetPlanning(string planningId)
        {
            return this.http.GetFromJsonAsync<PlanningDto>($"api/planning/{planningId}")!;
        }
    }
}

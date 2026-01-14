// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public class PlanningClientService : IPlanningClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/planning";

        public PlanningClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreatePlanning(PlanningDto planning)
        {
            var response = await this.http.PostAsJsonAsync(this.apiUrlBase, planning);

            if (planning.Id != null)
            {
                return planning.Id;
            }

            //Retrieve Planning ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedPlanning = Newtonsoft.Json.JsonConvert.DeserializeObject<PlanningDto>(responseJson);

            return updatedPlanning.Id.ToString();
        }

        public Task UpdatePlanning(PlanningDto planning)
        {
            return this.http.PutAsJsonAsync(this.apiUrlBase, planning);
        }

        public Task DeletePlanning(string planningId)
        {
            return this.http.DeleteAsync($"{this.apiUrlBase}/{planningId}");
        }

        public Task<PlanningDto> GetPlanning(string planningId)
        {
            return this.http.GetFromJsonAsync<PlanningDto>($"{this.apiUrlBase}/{planningId}")!;
        }

        public async Task<List<PlanningDto>> GetPlannings()
        {
            return await this.http.GetFromJsonAsync<List<PlanningDto>>(this.apiUrlBase) ?? new List<PlanningDto>();
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public class LevelClientService : ILevelClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/building";

        public LevelClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreateLevel(LevelDto level)
        {
            var response = await this.http.PostAsJsonAsync(this.apiUrlBase, level);

            //Retrieve Level ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedLevel = Newtonsoft.Json.JsonConvert.DeserializeObject<LevelDto>(responseJson);

            return updatedLevel.Id.ToString();
        }

        public Task UpdateLevel(LevelDto level)
        {
            return this.http.PutAsJsonAsync(this.apiUrlBase, level);
        }

        public Task DeleteLevel(string levelId)
        {
            return this.http.DeleteAsync($"{this.apiUrlBase}/{levelId}");
        }

        public Task<LevelDto> GetLevel(string levelId)
        {
            return this.http.GetFromJsonAsync<LevelDto>($"{this.apiUrlBase}/{levelId}")!;
        }

        public async Task<List<LevelDto>> GetLevels()
        {
            return await this.http.GetFromJsonAsync<List<LevelDto>>(this.apiUrlBase) ?? new List<LevelDto>();
        }
    }
}

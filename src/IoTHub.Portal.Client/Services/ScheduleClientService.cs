// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public class ScheduleClientService : IScheduleClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/schedule";

        public ScheduleClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreateSchedule(ScheduleDto schedule)
        {
            var response = await this.http.PostAsJsonAsync(this.apiUrlBase, schedule);

            //Retrieve Schedule ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedSchedule = Newtonsoft.Json.JsonConvert.DeserializeObject<ScheduleDto>(responseJson);

            return updatedSchedule.Id.ToString();
        }

        public Task UpdateSchedule(ScheduleDto schedule)
        {
            return this.http.PutAsJsonAsync(this.apiUrlBase, schedule);
        }

        public Task DeleteSchedule(string scheduleId)
        {
            return this.http.DeleteAsync($"{this.apiUrlBase}/{scheduleId}");
        }

        public Task<ScheduleDto> GetSchedule(string scheduleId)
        {
            return this.http.GetFromJsonAsync<ScheduleDto>($"{this.apiUrlBase}/{scheduleId}")!;
        }

        public async Task<List<ScheduleDto>> GetSchedules()
        {
            return await this.http.GetFromJsonAsync<List<ScheduleDto>>(this.apiUrlBase) ?? new List<ScheduleDto>();
        }
    }
}

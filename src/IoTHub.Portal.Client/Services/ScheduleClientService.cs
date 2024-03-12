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

        public ScheduleClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreateSchedule(ScheduleDto schedule)
        {
            var response = await this.http.PostAsJsonAsync("api/schedule", schedule);

            //Retrieve Device ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedSchedule = Newtonsoft.Json.JsonConvert.DeserializeObject<ScheduleDto>(responseJson);

            return updatedSchedule.Id.ToString();
        }

        public Task<ScheduleDto> GetSchedule(string scheduleId)
        {
            return this.http.GetFromJsonAsync<ScheduleDto>($"api/schedule/{scheduleId}")!;
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public class MenuEntryClientService : IMenuEntryClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/menu-entries";

        public MenuEntryClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreateMenuEntry(MenuEntryDto menuEntry)
        {
            var response = await this.http.PostAsJsonAsync(this.apiUrlBase, menuEntry);
            _ = response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var createdMenuEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<MenuEntryDto>(responseJson);

            return createdMenuEntry?.Id ?? menuEntry.Id;
        }

        public Task UpdateMenuEntry(MenuEntryDto menuEntry)
        {
            return this.http.PutAsJsonAsync($"{this.apiUrlBase}/{menuEntry.Id}", menuEntry);
        }

        public Task DeleteMenuEntry(string id)
        {
            return this.http.DeleteAsync($"{this.apiUrlBase}/{id}");
        }

        public Task<MenuEntryDto> GetMenuEntryById(string id)
        {
            return this.http.GetFromJsonAsync<MenuEntryDto>($"{this.apiUrlBase}/{id}")!;
        }

        public async Task<List<MenuEntryDto>> GetMenuEntries()
        {
            return await this.http.GetFromJsonAsync<List<MenuEntryDto>>(this.apiUrlBase) ?? new List<MenuEntryDto>();
        }

        public Task UpdateMenuEntryOrder(string id, int newOrder)
        {
            return this.http.PatchAsJsonAsync($"{this.apiUrlBase}/{id}/order", newOrder);
        }
    }
}

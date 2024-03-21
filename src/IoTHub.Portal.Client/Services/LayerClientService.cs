// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public class LayerClientService : ILayerClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/building";

        public LayerClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreateLayer(LayerDto layer)
        {
            var response = await this.http.PostAsJsonAsync(this.apiUrlBase, layer);

            //Retrieve Layer ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedLayer = Newtonsoft.Json.JsonConvert.DeserializeObject<LayerDto>(responseJson);

            return updatedLayer.Id.ToString();
        }

        public Task UpdateLayer(LayerDto layer)
        {
            return this.http.PutAsJsonAsync(this.apiUrlBase, layer);
        }

        public Task DeleteLayer(string layerId)
        {
            return this.http.DeleteAsync($"{this.apiUrlBase}/{layerId}");
        }

        public Task<LayerDto> GetLayer(string layerId)
        {
            return this.http.GetFromJsonAsync<LayerDto>($"{this.apiUrlBase}/{layerId}")!;
        }

        public async Task<List<LayerDto>> GetLayers()
        {
            return await this.http.GetFromJsonAsync<List<LayerDto>>(this.apiUrlBase) ?? new List<LayerDto>();
        }
    }
}

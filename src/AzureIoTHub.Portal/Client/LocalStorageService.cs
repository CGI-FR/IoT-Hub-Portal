// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client
{
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Microsoft.JSInterop;

    public class LocalStorageService
    {
        private readonly IJSRuntime js;

        public LocalStorageService(IJSRuntime js)
        {
            this.js = js;
        }

        public async Task<T> GetFromLocalStorage<T>(string key)
            where T: class
        {
            var jsonContent = await js.InvokeAsync<string>("localStorage.getItem", key);

            if (string.IsNullOrEmpty(jsonContent))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(jsonContent);
        }

        public async Task SetLocalStorage<T>(string key, T value)
        {
            var jsonContent = JsonSerializer.Serialize(value);

            await js.InvokeVoidAsync("localStorage.setItem", key, jsonContent);
        }
    }
}

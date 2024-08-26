// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Extensions
{
    using Microsoft.AspNetCore.Components.Forms;

    public static class IBrowserFileExtension
    {
        public static async Task<string> ToBase64(this IBrowserFile e, string contentType)
        {
            var buffer = new byte[e.Size];
            _ = await e.OpenReadStream().ReadAsync(buffer);

            return $"data:{contentType};base64,{Convert.ToBase64String(buffer)}";
        }
    }
}

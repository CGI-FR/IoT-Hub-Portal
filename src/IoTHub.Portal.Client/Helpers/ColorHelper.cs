// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Helpers
{
    public static class ColorHelper
    {
        public static string GetRandomColor()
        {
            var random = new Random();
            var color = $"#{random.Next(0x1000000):X6}";
            return color;
        }
    }
}

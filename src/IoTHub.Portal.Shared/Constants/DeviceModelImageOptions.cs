// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Constants
{
    using System.IO;
    using System.Text;

    public static class DeviceModelImageOptions
    {
        public const string ImageContainerName = "device-images";

        public const string DefaultImageName = "default-template-icon";

        public const string DefaultImage =
            "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABwAAAAQCAYAAAAFzx/vAAAABmJLR0QA/wD/AP+gvaeTAAABF0lEQVQ4jb3TwStEURTH8c97jZKkbKwtxVIpomyxVLKRJTb+CitbEqX8Cbb+BAtZ2ashslBIzTQWz+Ldlzev92YxM8+3Tt1zzu387j33HrqJ8IBkSHaPOC/Q5WAXLTSC+CAW4wt7KpjEKxaqNvTBHN4wVZY8w2nB77eVbcyEOse4KorN40V6S5jFzwCCCa5DrTE8YjUTi3GLndwBbgYUy2w51NuUfsYRoY3tIQn0shYu4R3bxR7XwBY+BPX/ImmUBMdxGA5ygu9hKkahcJSLXUgfOJL+1v1c7gAT+MR5zq8i25dResNpHIX1RiE3Kh2dTsGvolMWLL7hEu6CLfYo1g9JmWCdJDGe/Q1onazgCdbRVP/gN7H2C3wfojH39G0+AAAAAElFTkSuQmCC";

        public static MemoryStream DefaultImageStream => new(Encoding.UTF8.GetBytes(DefaultImage));
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Options
{
    public class DeviceModelImageOptions
    {
        public Uri BaseUri { get; set; } = default!;

        public string ImageContainerName { get; } = "device-images-2";

        public string DefaultImageName { get; } = "default-template-icon.png";
    }
}

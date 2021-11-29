﻿// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface ISensorImageManager
    {
        Uri ComputeImageUri(string sensorModelName);

        Task<Uri> ChangeSensorImageAsync(string sensorModelName, Stream stream);

        Task InitializeDefaultImageBlob();
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    public class ImportResultLine
    {
        public int LineNumber { get; set; }

        public string DeviceId { get; set; } = default!;

        public string Message { get; set; } = default!;

        public bool IsErrorMessage { get; set; }
    }
}

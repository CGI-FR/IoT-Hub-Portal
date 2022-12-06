// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Managers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.v10;

    public interface IExportManager
    {
        Task ExportDeviceList(Stream stream);

        Task ExportTemplateFile(Stream stream);

        Task<IEnumerable<ImportResultLine>> ImportDeviceList(Stream stream);
    }
}

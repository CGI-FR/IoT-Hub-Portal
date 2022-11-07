// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IExportManager
    {
        Task<Stream> ExportDeviceList();
    }
}
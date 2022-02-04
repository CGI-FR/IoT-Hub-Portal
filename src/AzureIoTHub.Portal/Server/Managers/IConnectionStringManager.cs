// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Threading.Tasks;

    public interface IConnectionStringManager
    {
        Task<string> GetSymmetricKey(string deviceId, string deviceType);
    }
}

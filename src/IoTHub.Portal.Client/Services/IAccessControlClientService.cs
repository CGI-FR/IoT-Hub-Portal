// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public interface IAccessControlClientService
    {
        Task<PaginationResult<AccessControlModel>> GetAccessControls(string continuationUri);
        Task<AccessControlModel> Create(AccessControlModel model);
        Task Delete(string id);
    }
}

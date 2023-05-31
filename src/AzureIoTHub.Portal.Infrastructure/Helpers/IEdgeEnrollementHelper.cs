// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Helpers
{
    public interface IEdgeEnrollementHelper
    {
        string GetEdgeEnrollementTemplate(string templateName);
    }
}

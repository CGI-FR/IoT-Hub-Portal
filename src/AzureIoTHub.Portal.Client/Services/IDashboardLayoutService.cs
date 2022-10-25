// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;

    public interface IDashboardLayoutService
    {
        event EventHandler RefreshDashboardOccurred;

        void RefreshDashboard();
    }
}

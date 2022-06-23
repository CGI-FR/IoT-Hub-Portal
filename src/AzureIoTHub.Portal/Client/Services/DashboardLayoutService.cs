// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;

    public class DashboardLayoutService : IDashboardLayoutService
    {
        public event EventHandler RefreshDashboardOccurred;

        public void RefreshDashboard()
        {
            OnRefreshDashboardOccurred();
        }

        private void OnRefreshDashboardOccurred() => RefreshDashboardOccurred?.Invoke(this, EventArgs.Empty);
    }
}

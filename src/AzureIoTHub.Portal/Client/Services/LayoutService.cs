// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;

    public class LayoutService : ILayoutService
    {
        public bool IsDrawerOpen { get; set; } = true;

        public event EventHandler MajorUpdateOccurred;

        public void ToggleDrawer()
        {
            IsDrawerOpen = !IsDrawerOpen;
            OnMajorUpdateOccurred();
        }

        private void OnMajorUpdateOccurred() => MajorUpdateOccurred?.Invoke(this, EventArgs.Empty);
    }
}

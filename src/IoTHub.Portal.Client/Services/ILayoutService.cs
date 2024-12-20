// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public interface ILayoutService
    {
        bool IsDrawerOpen { get; set; }
        bool IsDarkMode { get; set; }

        event EventHandler MajorUpdateOccurred;

        Task LoadLayoutConfigurationFromLocalStorage();

        void ToggleDrawer();

        Task ToggleDarkMode();

        void SetDarkMode(bool value);

        bool GetNavGroupExpanded(string key);

        Task SetNavGroupExpanded(string key, bool value);
    }
}

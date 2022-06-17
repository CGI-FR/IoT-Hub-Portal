// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Constants;
    using Blazored.LocalStorage;

    public class LayoutService : ILayoutService
    {
        private readonly ILocalStorageService localStorageService;

        public LayoutService(ILocalStorageService localStorageService)
        {
            this.localStorageService = localStorageService;
            CollapsibleNavMenu = new Dictionary<string, bool>();
        }

        public bool IsDrawerOpen { get; set; } = true;
        public bool IsDarkMode { get; set; }

        public Dictionary<string, bool> CollapsibleNavMenu { get; set; }

        public event EventHandler MajorUpdateOccurred;

        public async Task LoadLayoutConfigurationFromLocalStorage()
        {
            IsDarkMode = await this.localStorageService.GetItemAsync<bool>(LocalStorageKey.DarkTheme);
            CollapsibleNavMenu = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>(LocalStorageKey.CollapsibleNavMenu) ?? new Dictionary<string, bool>();
        }

        public void ToggleDrawer()
        {
            IsDrawerOpen = !IsDrawerOpen;
            OnMajorUpdateOccurred();
        }

        public async Task ToggleDarkMode()
        {
            IsDarkMode = !IsDarkMode;
            await this.localStorageService.SetItemAsync(LocalStorageKey.DarkTheme, IsDarkMode);
            OnMajorUpdateOccurred();
        }

        public void SetDarkMode(bool value)
        {
            IsDarkMode = value;
        }

        public bool GetNavGroupExpanded(string key)
        {
            return CollapsibleNavMenu == null || !CollapsibleNavMenu.TryGetValue(key, out var result) || result;
        }

        public async Task SetNavGroupExpanded(string key, bool value)
        {
            if (CollapsibleNavMenu.ContainsKey(key))
            {
                CollapsibleNavMenu[key] = value;
            }
            else
            {
                CollapsibleNavMenu.Add(key, value);
            }

            await this.localStorageService.SetItemAsync(LocalStorageKey.CollapsibleNavMenu, CollapsibleNavMenu);
        }

        private void OnMajorUpdateOccurred() => MajorUpdateOccurred?.Invoke(this, EventArgs.Empty);
    }
}

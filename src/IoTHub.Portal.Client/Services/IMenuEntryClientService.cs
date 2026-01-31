// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IMenuEntryClientService
    {
        Task<string> CreateMenuEntry(MenuEntryDto menuEntry);
        Task UpdateMenuEntry(MenuEntryDto menuEntry);
        Task DeleteMenuEntry(string id);
        Task<MenuEntryDto> GetMenuEntryById(string id);
        Task<List<MenuEntryDto>> GetMenuEntries();
        Task UpdateMenuEntryOrder(string id, int newOrder);
    }
}

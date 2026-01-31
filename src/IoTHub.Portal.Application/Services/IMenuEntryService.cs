// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IMenuEntryService
    {
        Task<MenuEntryDto> CreateMenuEntry(MenuEntryDto menuEntryDto);
        Task<IEnumerable<MenuEntryDto>> GetAllMenuEntries();
        Task<MenuEntryDto?> GetMenuEntryById(string id);
        Task UpdateMenuEntry(MenuEntryDto menuEntryDto);
        Task DeleteMenuEntry(string id);
        Task UpdateMenuEntryOrder(string id, int newOrder);
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v10;

    public class MenuEntryService : IMenuEntryService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMenuEntryRepository menuEntryRepository;

        public MenuEntryService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IMenuEntryRepository menuEntryRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.menuEntryRepository = menuEntryRepository;
        }

        public async Task<MenuEntryDto> CreateMenuEntry(MenuEntryDto menuEntryDto)
        {
            // Validate name length
            if (string.IsNullOrWhiteSpace(menuEntryDto.Name))
            {
                throw new ArgumentException("Menu entry name is required", nameof(menuEntryDto));
            }

            if (menuEntryDto.Name.Length > 100)
            {
                throw new ArgumentException("Menu entry name must not exceed 100 characters", nameof(menuEntryDto));
            }

            // Validate URL format
            if (string.IsNullOrWhiteSpace(menuEntryDto.Url))
            {
                throw new ArgumentException("Menu entry URL is required", nameof(menuEntryDto));
            }

            if (!Uri.TryCreate(menuEntryDto.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Menu entry URL must be a valid HTTP or HTTPS URL", nameof(menuEntryDto));
            }

            // Check for duplicate name
            var existing = await this.menuEntryRepository.GetByNameAsync(menuEntryDto.Name);
            if (existing != null)
            {
                throw new InvalidOperationException($"A menu entry with the name '{menuEntryDto.Name}' already exists");
            }

            // Determine if URL is external
            menuEntryDto.IsExternal = !menuEntryDto.Url.StartsWith("/", StringComparison.OrdinalIgnoreCase);

            var menuEntry = this.mapper.Map<MenuEntry>(menuEntryDto);
            menuEntry.CreatedAt = DateTime.UtcNow;
            menuEntry.UpdatedAt = DateTime.UtcNow;

            await this.menuEntryRepository.InsertAsync(menuEntry);
            await this.unitOfWork.SaveAsync();

            return this.mapper.Map<MenuEntryDto>(menuEntry);
        }

        public async Task<IEnumerable<MenuEntryDto>> GetAllMenuEntries()
        {
            var menuEntries = await this.menuEntryRepository.GetAllAsync();

            return menuEntries
                .OrderBy(x => x.Order)
                .Select(x => this.mapper.Map<MenuEntryDto>(x))
                .ToList();
        }

        public async Task<MenuEntryDto?> GetMenuEntryById(string id)
        {
            var menuEntry = await this.menuEntryRepository.GetByIdAsync(id);
            return menuEntry != null ? this.mapper.Map<MenuEntryDto>(menuEntry) : null;
        }

        public async Task UpdateMenuEntry(MenuEntryDto menuEntryDto)
        {
            var menuEntry = await this.menuEntryRepository.GetByIdAsync(menuEntryDto.Id);

            if (menuEntry == null)
            {
                throw new ResourceNotFoundException($"The menu entry with id {menuEntryDto.Id} doesn't exist");
            }

            // Validate name length
            if (string.IsNullOrWhiteSpace(menuEntryDto.Name))
            {
                throw new ArgumentException("Menu entry name is required", nameof(menuEntryDto));
            }

            if (menuEntryDto.Name.Length > 100)
            {
                throw new ArgumentException("Menu entry name must not exceed 100 characters", nameof(menuEntryDto));
            }

            // Check for duplicate name (excluding current entry)
            var existing = await this.menuEntryRepository.GetByNameAsync(menuEntryDto.Name);
            if (existing != null && existing.Id != menuEntryDto.Id)
            {
                throw new InvalidOperationException($"A menu entry with the name '{menuEntryDto.Name}' already exists");
            }

            // Validate URL format
            if (string.IsNullOrWhiteSpace(menuEntryDto.Url))
            {
                throw new ArgumentException("Menu entry URL is required", nameof(menuEntryDto));
            }

            if (!Uri.TryCreate(menuEntryDto.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Menu entry URL must be a valid HTTP or HTTPS URL", nameof(menuEntryDto));
            }

            // Determine if URL is external
            menuEntryDto.IsExternal = !menuEntryDto.Url.StartsWith("/", StringComparison.OrdinalIgnoreCase);
            menuEntryDto.UpdatedAt = DateTime.UtcNow;

            _ = this.mapper.Map(menuEntryDto, menuEntry);
            menuEntry.UpdatedAt = DateTime.UtcNow;

            this.menuEntryRepository.Update(menuEntry);
            await this.unitOfWork.SaveAsync();
        }

        public async Task DeleteMenuEntry(string id)
        {
            var menuEntry = await this.menuEntryRepository.GetByIdAsync(id);

            if (menuEntry == null)
            {
                throw new ResourceNotFoundException($"The menu entry with id {id} doesn't exist");
            }

            this.menuEntryRepository.Delete(id);
            await this.unitOfWork.SaveAsync();
        }

        public async Task UpdateMenuEntryOrder(string id, int newOrder)
        {
            var menuEntry = await this.menuEntryRepository.GetByIdAsync(id);

            if (menuEntry == null)
            {
                throw new ResourceNotFoundException($"The menu entry with id {id} doesn't exist");
            }

            menuEntry.Order = newOrder;
            menuEntry.UpdatedAt = DateTime.UtcNow;

            this.menuEntryRepository.Update(menuEntry);
            await this.unitOfWork.SaveAsync();
        }
    }
}

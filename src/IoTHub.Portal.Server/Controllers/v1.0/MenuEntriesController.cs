// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/menu-entries")]
    [ApiExplorerSettings(GroupName = "Menu Entries")]
    public class MenuEntriesController : ControllerBase
    {
        private readonly IMenuEntryService menuEntryService;

        public MenuEntriesController(IMenuEntryService menuEntryService)
        {
            this.menuEntryService = menuEntryService;
        }

        /// <summary>
        /// Creates a new menu entry.
        /// </summary>
        /// <param name="menuEntryDto">The menu entry data.</param>
        /// <returns>The created menu entry.</returns>
        [Authorize("menuentry:write")]
        [HttpPost(Name = "POST Create menu entry")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MenuEntryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMenuEntry([FromBody] MenuEntryDto menuEntryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await this.menuEntryService.CreateMenuEntry(menuEntryDto);
                return CreatedAtAction(nameof(GetMenuEntry), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the menu entry");
            }
        }

        /// <summary>
        /// Gets all menu entries.
        /// </summary>
        /// <returns>A list of menu entries ordered by their Order property.</returns>
        [Authorize("menuentry:read")]
        [HttpGet(Name = "GET All menu entries")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MenuEntryDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<MenuEntryDto>>> GetAllMenuEntries()
        {
            var menuEntries = await this.menuEntryService.GetAllMenuEntries();
            return Ok(menuEntries);
        }

        /// <summary>
        /// Gets a specific menu entry by ID.
        /// </summary>
        /// <param name="id">The menu entry ID.</param>
        /// <returns>The menu entry.</returns>
        [Authorize("menuentry:read")]
        [HttpGet("{id}", Name = "GET Menu entry by ID")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MenuEntryDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMenuEntry(string id)
        {
            var menuEntry = await this.menuEntryService.GetMenuEntryById(id);

            if (menuEntry == null)
            {
                return NotFound($"Menu entry with ID {id} not found");
            }

            return Ok(menuEntry);
        }

        /// <summary>
        /// Updates an existing menu entry.
        /// </summary>
        /// <param name="id">The menu entry ID.</param>
        /// <param name="menuEntryDto">The updated menu entry data.</param>
        /// <returns>No content on success.</returns>
        [Authorize("menuentry:write")]
        [HttpPut("{id}", Name = "PUT Update menu entry")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMenuEntry(string id, [FromBody] MenuEntryDto menuEntryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != menuEntryDto.Id)
                {
                    return BadRequest("ID in URL does not match ID in body");
                }

                await this.menuEntryService.UpdateMenuEntry(menuEntryDto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Domain.Exceptions.ResourceNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the menu entry");
            }
        }

        /// <summary>
        /// Deletes a menu entry.
        /// </summary>
        /// <param name="id">The menu entry ID.</param>
        /// <returns>No content on success.</returns>
        [Authorize("menuentry:write")]
        [HttpDelete("{id}", Name = "DELETE Remove menu entry")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteMenuEntry(string id)
        {
            try
            {
                await this.menuEntryService.DeleteMenuEntry(id);
                return NoContent();
            }
            catch (Domain.Exceptions.ResourceNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the menu entry");
            }
        }

        /// <summary>
        /// Updates the order of a menu entry.
        /// </summary>
        /// <param name="id">The menu entry ID.</param>
        /// <param name="newOrder">The new order value.</param>
        /// <returns>No content on success.</returns>
        [Authorize("menuentry:write")]
        [HttpPatch("{id}/order", Name = "PATCH Update menu entry order")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMenuEntryOrder(string id, [FromBody] int newOrder)
        {
            try
            {
                await this.menuEntryService.UpdateMenuEntryOrder(id, newOrder);
                return Ok();
            }
            catch (Domain.Exceptions.ResourceNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the menu entry order");
            }
        }
    }
}

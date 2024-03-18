// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public interface ILevelService
    {
        Task<LevelDto> CreateLevel(LevelDto level);
        Task UpdateLevel(LevelDto level);
        Task DeleteLevel(string levelId);
        Task<Level> GetLevel(string levelId);
        Task<IEnumerable<LevelDto>> GetLevels();
    }
}

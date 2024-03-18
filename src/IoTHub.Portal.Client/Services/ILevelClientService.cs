// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public interface ILevelClientService
    {
        Task<string> CreateLevel(LevelDto level);
        Task UpdateLevel(LevelDto level);
        Task DeleteLevel(string modelId);
        Task<LevelDto> GetLevel(string levelId);
        Task<List<LevelDto>> GetLevels();
    }
}

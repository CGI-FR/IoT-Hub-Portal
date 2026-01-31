// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public interface IPlanningClientService
    {
        Task<string> CreatePlanning(PlanningDto planning);
        Task UpdatePlanning(PlanningDto planning);
        Task DeletePlanning(string modelId);
        Task<PlanningDto> GetPlanning(string planningId);
        Task<List<PlanningDto>> GetPlannings();
    }
}

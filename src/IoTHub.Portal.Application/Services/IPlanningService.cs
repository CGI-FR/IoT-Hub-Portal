// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IPlanningService
    {
        Task<PlanningDto> CreatePlanning(PlanningDto planning);
        Task UpdatePlanning(PlanningDto planning);
        Task DeletePlanning(string planningId);
        Task<Planning> GetPlanning(string planningId);
        Task<IEnumerable<PlanningDto>> GetPlannings();
    }
}

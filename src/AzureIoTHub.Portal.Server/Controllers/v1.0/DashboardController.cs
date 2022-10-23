// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v1._0
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models.v1._0;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/dashboard")]
    [ApiExplorerSettings(GroupName = "Metrics")]
    public class DashboardController : ControllerBase
    {
        private readonly PortalMetric portalMetric;

        public DashboardController(PortalMetric portalMetric)
        {
            this.portalMetric = portalMetric;
        }

        [HttpGet("metrics", Name = "Get Portal Metrics")]
        public ActionResult<PortalMetric> GetPortalMetrics()
        {
            return this.portalMetric;
        }
    }
}

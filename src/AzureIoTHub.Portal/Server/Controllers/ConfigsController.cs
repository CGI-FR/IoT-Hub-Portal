// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]

    public class ConfigsController : ControllerBase
    {
        private readonly ILogger<ConfigsController> logger;

        private readonly RegistryManager registryManager;

        public ConfigsController(ILogger<ConfigsController> logger, RegistryManager registryManager)
        {
            this.logger = logger;
            this.registryManager = registryManager;
        }

        private static long RetrieveMetricValue(Configuration item, string metricName)
        {
            if (item.SystemMetrics.Results.Keys.Contains(metricName))
                return item.SystemMetrics.Results[metricName];
            else
                return -1;
        }

        [HttpGet]
        public async Task<IEnumerable<ConfigListItem>> Get()
        {
            List<Configuration> configList = (List<Configuration>)await this.registryManager.GetConfigurationsAsync(10);
            var results = new List<ConfigListItem>();

            foreach (Configuration config in configList)
            {
                var result = new ConfigListItem
                {
                    ConfigurationID = config.Id,
                    Conditions = config.TargetCondition,
                    MetricsTargeted = RetrieveMetricValue(config, "targetedCount"),
                    MetricsApplied = RetrieveMetricValue(config, "appliedCount"),
                    MetricsSuccess = RetrieveMetricValue(config, "reportedSuccessfulCount"),
                    MetricsFailure = RetrieveMetricValue(config, "reportedFailedCount"),
                    Priority = config.Priority,
                    CreationDate = config.CreatedTimeUtc
                };
                results.Add(result);
            }

            return results;
        }
    }
}

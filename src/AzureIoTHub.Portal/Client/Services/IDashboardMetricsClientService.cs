// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Threading.Tasks;
    using Portal.Shared.Models.v1._0;

    public interface IDashboardMetricsClientService
    {
        Task<PortalMetric> GetPortalMetrics();
    }
}

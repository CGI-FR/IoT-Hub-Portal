// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Shared.Models.v1._0;

    public class DashboardMetricsClientService : IDashboardMetricsClientService
    {
        private readonly HttpClient http;

        public DashboardMetricsClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PortalMetricDto> GetPortalMetrics()
        {
            return this.http.GetFromJsonAsync<PortalMetricDto>("api/dashboard/metrics")!;
        }
    }
}

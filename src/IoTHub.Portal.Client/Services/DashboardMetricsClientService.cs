// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public class DashboardMetricsClientService : IDashboardMetricsClientService
    {
        private readonly HttpClient http;

        public DashboardMetricsClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PortalMetric> GetPortalMetrics()
        {
            return this.http.GetFromJsonAsync<PortalMetric>("api/dashboard/metrics")!;
        }
    }
}

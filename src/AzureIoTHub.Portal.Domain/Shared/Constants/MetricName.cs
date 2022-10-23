// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Shared.Constants
{
    public static class MetricName
    {
        private const string Prefix = "iot_hub_portal";

        public const string DeviceCount = $"{Prefix}_device_count";

        public const string ConnectedDeviceCount = $"{Prefix}_connected_device_count";

        public const string EdgeDeviceCount = $"{Prefix}_edge_device_count";

        public const string ConnectedEdgeDeviceCount = $"{Prefix}_connected_edge_device_count";

        public const string FailedDeploymentCount = $"{Prefix}_failed_deployment_count";

        public const string ConcentratorCount = $"{Prefix}_concentrator_count";

        public const string ConnectedConcentratorCount = $"{Prefix}_connected_concentrator_count";
    }
}

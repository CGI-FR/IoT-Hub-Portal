// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0
{
    public class PortalMetric
    {
        public int DeviceCount { get; set; }

        public int ConnectedDeviceCount { get; set; }

        public int EdgeDeviceCount { get; set; }

        public int ConnectedEdgeDeviceCount { get; set; }

        public int FailedDeploymentCount { get; set; }

        public int ConcentratorCount { get; set; }
    }
}

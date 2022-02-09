// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.Concentrator;
    using Microsoft.Azure.Devices.Shared;

    public interface IConcentratorTwinMapper
    {
        Concentrator CreateDeviceDetails(Twin twin);

        Task UpdateTwin(Twin twin, Concentrator item);
    }
}

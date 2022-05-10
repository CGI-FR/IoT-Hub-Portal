// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Extensions
{
    using System;
    using System.Text;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public static class DeviceModelCommandExtensions
    {
        public static dynamic ToDynamic(this DeviceModelCommand command)
        {
            return command.Confirmed
                ? (new
                {
                    rawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Frame)),
                    fport = command.Port,
                    confirmed = command.Confirmed
                })
                : (new
                {
                    rawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Frame)),
                    fport = command.Port
                });
        }
    }
}

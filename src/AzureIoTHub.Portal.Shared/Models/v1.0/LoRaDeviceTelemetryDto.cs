// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    using System;
    using System.Collections.Generic;

    public class LoRaDeviceTelemetryDto
    {
        public string Id { get; set; }

        public DateTime EnqueuedTime { get; set; }

        public LoRaTelemetryDto Telemetry { get; set; }
    }

    public class LoRaTelemetryDto
    {
        public ulong Time { get; set; }

        public uint GpsTime { get; set; }

        public double Freq { get; set; }

        public uint Chan { get; set; }

        public uint Rfch { get; set; }

        public string Modu { get; set; }

        public string Datr { get; set; }

        public double Rssi { get; set; }

        public float Lsnr { get; set; }

        public object Data { get; set; }

        public uint Port { get; set; }

        public ushort Fcnt { get; set; }

        public long Edgets { get; set; }

        public string Rawdata { get; set; }

        public string DeviceEUI { get; set; }

        public string GatewayID { get; set; }

        public string StationEui { get; set; }

        public bool? DupMsg { get; set; }

        public Dictionary<string, object> ExtraData { get; } = new();
    }
}

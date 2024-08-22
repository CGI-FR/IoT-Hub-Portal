// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;
    using System.Collections.Generic;

    public class LoRaDeviceTelemetryDto
    {
        public string Id { get; set; } = default!;

        public DateTime EnqueuedTime { get; set; }

        public LoRaTelemetryDto Telemetry { get; set; } = new LoRaTelemetryDto();
    }

    public class LoRaTelemetryDto
    {
        public ulong Time { get; set; }

        public uint GpsTime { get; set; }

        public double Freq { get; set; }

        public uint Chan { get; set; }

        public uint Rfch { get; set; }

        public string Modu { get; set; } = default!;

        public string Datr { get; set; } = default!;

        public double Rssi { get; set; }

        public float Lsnr { get; set; }

        public object Data { get; set; } = new object();

        public uint Port { get; set; }

        public ushort Fcnt { get; set; }

        public long Edgets { get; set; }

        public string Rawdata { get; set; } = default!;

        public string DeviceEUI { get; set; } = default!;

        public string GatewayID { get; set; } = default!;

        public string StationEui { get; set; } = default!;

        public bool? DupMsg { get; set; }

        public Dictionary<string, object> ExtraData { get; } = new();
    }
}

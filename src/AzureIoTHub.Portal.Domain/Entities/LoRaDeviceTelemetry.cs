// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using System.Text.Json.Serialization;
    using Base;

    public class LoRaDeviceTelemetry : EntityBase
    {
        public DateTime EnqueuedTime { get; set; }

        public LoRaTelemetry Telemetry { get; set; }
    }

    public class LoRaTelemetry
    {
        [JsonPropertyName("time")]
        public ulong Time { get; set; }

        [JsonPropertyName("tmms")]
        public uint GpsTime { get; set; }

        [JsonPropertyName("freq")]
        public double Freq { get; set; }

        [JsonPropertyName("chan")]
        public uint Chan { get; set; }

        [JsonPropertyName("rfch")]
        public uint Rfch { get; set; }

        [JsonPropertyName("modu")]
        public string Modu { get; set; }

        [JsonPropertyName("datr")]
        public string Datr { get; set; }

        [JsonPropertyName("rssi")]
        public double Rssi { get; set; }

        [JsonPropertyName("lsnr")]
        public float Lsnr { get; set; }

        [JsonPropertyName("data")]
        public object Data { get; set; }

        [JsonPropertyName("port")]
        public uint Port { get; set; }

        [JsonPropertyName("fcnt")]
        public ushort Fcnt { get; set; }

        [JsonPropertyName("edgets")]
        public long Edgets { get; set; }

        [JsonPropertyName("rawdata")]
        public string Rawdata { get; set; }

        [JsonPropertyName("eui")]
        public string DeviceEUI { get; set; }

        [JsonPropertyName("gatewayid")]
        public string GatewayID { get; set; }

        [JsonPropertyName("stationeui")]
        public string StationEui { get; set; }

        [JsonPropertyName("dupmsg")]
        public bool? DupMsg { get; set; }
    }
}

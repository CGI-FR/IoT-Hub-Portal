// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN
{
    using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class LoRaDeviceModelBase : DeviceModel
    {
        /// <summary>
        /// The status of OTAA setting.
        /// </summary>
        public bool UseOTAA { get; set; }

        /// <summary>
        /// The device OTAA Application eui.
        /// </summary>
        // [Required(ErrorMessage = "The OTAA App EUI is required.")]
        public string AppEUI { get; set; }

        /// <summary>
        /// The sensor decoder API Url.
        /// </summary>
        public string SensorDecoder { get; set; }

        /// <summary>
        /// Allows disabling the downstream (cloud to device) for a device.
        /// By default downstream messages are enabled.
        /// </summary>
        public bool? Downlink { get; set; }

        /// <summary>
        /// Allows setting the device preferred receive window (RX1 or RX2).
        /// The default preferred receive window is 1.
        /// </summary>
        public int? PreferredWindow { get; set; }

        /// <summary>
        /// Allows controlling the handling of duplicate messages received by multiple gateways.
        /// The default is None.
        /// </summary>
        public string Deduplication { get; set; }

        /// <summary>
        /// Allows setting an offset between received Datarate and retransmit datarate as specified in the LoRa Specifiations.
        /// Valid for OTAA devices.
        /// If an invalid value is provided the network server will use default value 0.
        /// </summary>
        public int? RX1DROffset { get; set; }

        /// <summary>
        /// Allows setting a custom Datarate for second receive windows.
        /// Valid for OTAA devices.
        /// If an invalid value is provided the network server will use default value 0 (DR0).
        /// </summary>
        public int? RX2DataRate { get; set; }

        /// <summary>
        /// Allows setting a custom wait time between receiving and transmission as specified in the specification.
        /// </summary>
        public int? RXDelay { get; set; }

        /// <summary>
        /// Allows to disable the relax mode when using ABP.
        /// By default relaxed mode is enabled.
        /// </summary>
        public bool? ABPRelaxMode { get; set; }

        /// <summary>
        /// Allows to explicitly specify a frame counter up start value.
        /// If the device joins, this value will be used to validate the first frame and initialize the server state for the device.
        /// </summary>
        public int? FCntUpStart { get; set; }

        /// <summary>
        /// Allows to explicitly specify a frame counter down start value.
        /// </summary>
        public int? FCntDownStart { get; set; }

        /// <summary>
        /// Allow the usage of 32bit counters on your device.
        /// </summary>
        public bool? Supports32BitFCnt { get; set; }

        /// <summary>
        /// Allows to reset the frame counters to the FCntUpStart/FCntDownStart values respectively.
        /// </summary>
        public int? FCntResetCounter { get; set; }

        /// <summary>
        /// Allows defining a sliding expiration to the connection between the leaf device and IoT/Edge Hub.
        /// The default is none, which causes the connection to not be dropped.
        /// </summary>
        public int? KeepAliveTimeout { get; set; }

        public LoRaDeviceModelBase()
        {
            this.Downlink = true;
            this.PreferredWindow = 1;
            this.Deduplication = "None";
            this.ABPRelaxMode = true;
        }
    }
}

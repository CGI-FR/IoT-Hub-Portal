// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Device details.
    /// </summary>
    public class DeviceDetails : IDeviceDetails
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        //[Required(ErrorMessage = "The device should have a unique identifier.")]
        [MaxLength(ErrorMessage = "The device identifier should be up to 128 characters long.")]
        [RegularExpression("^[a-zA-Z0-9\\-.+%_#*?!(),:=@$']{1,128}$", ErrorMessage = "The device identifier should be of ASCII 7-bit alphanumeric characters plus certain special characters: - . + % _ # * ? ! ( ) , : = @ $ '.")]
        public virtual string DeviceID { get; set; } = default!;

        /// <summary>
        /// The name of the device.
        /// </summary>
        [Required(ErrorMessage = "The device should have a name.")]
        public string DeviceName { get; set; } = default!;

        /// <summary>
        /// The model identifier.
        /// </summary>
        [Required(ErrorMessage = "The device should use a model.")]
        public string ModelId { get; set; } = default!;

        /// <summary>
        /// The model name. (For AWS)
        /// </summary>
        public string ModelName { get; set; } = default!;

        /// <summary>
        /// The device model image Url.
        /// </summary>
        public Uri ImageUrl { get; set; } = default!;

        /// <summary>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The status updated time.
        /// </summary>
        public DateTime StatusUpdatedTime { get; set; }

        /// <summary>
        /// List of custom device tags and their values.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();

        /// <summary>
        ///   <c>true</c> if this instance is lorawan; otherwise, <c>false</c>.
        /// </summary>
        public virtual bool IsLoraWan { get; }

        /// <summary>
        /// Labels
        /// </summary>
        public List<LabelDto> Labels { get; set; } = new();
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Device search model.
    /// </summary>
    public class SearchModel
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        [Required(ErrorMessage = "The device identifier is required.")]
        public string? DeviceId { get; set; }

        /// <summary>
        /// The device status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// The device type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchModel"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="status">The device status.</param>
        /// <param name="type">The device type.</param>
        public SearchModel(string? id = null, string? status = null, string? type = null)
        {
            DeviceId = id;
            Status = status;
            Type = type;
        }
    }
}

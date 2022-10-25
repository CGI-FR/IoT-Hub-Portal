// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Base;

    public class DeviceTag : EntityBase
    {
        [NotMapped] public string Name => Id;

        public string Label { get; set; }

        public bool Required { get; set; }

        public bool Searchable { get; set; }
    }
}

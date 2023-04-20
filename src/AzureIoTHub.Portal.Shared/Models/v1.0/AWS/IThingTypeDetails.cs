// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.AWS
{
    using System.Collections.Generic;

    public interface IThingTypeDetails
    {
        /// <summary>
        /// The thing type identifier.
        /// </summary>
        public string ThingTypeID { get; set; }

        /// <summary>
        /// The thing type name.
        /// </summary>
        public string ThingTypeName { get; set; }

        /// <summary>
        /// The thing type description.
        /// </summary>
        public string ThingTypeDescription { get; set; }

        /// <summary>
        /// List of custom thing type tags and their values
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// List of thing type searchable attributes
        /// </summary>
        public List<ThingTypeSearchableAttDto> ThingTypeSearchableAttDtos { get; set; }
    }
}

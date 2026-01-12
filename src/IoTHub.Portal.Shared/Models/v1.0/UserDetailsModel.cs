// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    //using System.Collections.Generic;
    public class UserDetailsModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string Name { get; set; }
        public string FamilyName { get; set; }
        public string Avatar { get; set; }
        public string PrincipalId { get; set; }
        //public ICollection<AccessControlModel> AccessControls { get; set; } = new List<AccessControlModel>();
    }
}

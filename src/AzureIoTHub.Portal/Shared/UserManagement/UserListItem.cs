// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.UserManagement
{
    public class UserListItem
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string DisplayName { get; set; }

        public string UserType { get; set; }

        public string Role { get; set; }
    }
}

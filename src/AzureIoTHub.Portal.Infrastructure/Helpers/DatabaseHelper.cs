// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Helpers
{
    using System;
    using Microsoft.EntityFrameworkCore;

    public static class DatabaseHelper
    {
        public static ServerVersion GetMySqlServerVersion(string mySQLConnectionString)
        {
            try
            {
                return ServerVersion.AutoDetect(mySQLConnectionString);
            }
            catch
            {
                return new MySqlServerVersion(new Version(8, 0, 32));
            }
        }
    }
}

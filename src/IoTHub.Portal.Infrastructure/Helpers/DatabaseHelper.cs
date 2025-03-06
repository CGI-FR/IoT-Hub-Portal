// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Helpers
{
    public static class DatabaseHelper
    {
        public static ServerVersion GetMySqlServerVersion(string mySQLConnectionString)
        {
            try
            {
                return ServerVersion.AutoDetect(mySQLConnectionString);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return new MySqlServerVersion(new Version(8, 0, 32));
            }
        }
    }
}

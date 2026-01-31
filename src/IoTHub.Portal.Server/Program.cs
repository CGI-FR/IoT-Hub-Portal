// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                Host.CreateDefaultBuilder(args)
                    .Build().Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => _ = webBuilder.UseStartup<Startup>());
    }
}

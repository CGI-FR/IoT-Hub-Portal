// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.E2E
{
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using NUnit.Framework;
    using OpenQA.Selenium;

    public class E2ETest
    {
        protected IConfiguration Configuration { get; } = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables()
                .Build();

        protected virtual AutoFixture.Fixture Fixture { get; } = new();

        [TearDown]
        public virtual void TearDown()
        {
            this.AddBorwserLogs();

            WebDriverFactory.Quit();
        }

        private void AddBorwserLogs()
        {
            var errors = "\n \n*** Errors ***\n \n";
            var errorLogs = GetBrowserError();

            if (errorLogs.Count != 0)
            {
                foreach (var logEntry in errorLogs)
                {
                    errors += $"{logEntry}\n";
                }

                // Add errors to TestContext
                TestContext.WriteLine($"{errors}\nNumber of browser errors is: {errorLogs.Count}");
            }
        }

        private static List<string> GetBrowserError()
        {
            var logs = WebDriverFactory.Default.Manage().Logs;

            var logEntries = logs.GetLog(LogType.Browser); // LogType: Browser, Server, Driver, Client and Profiler
            var errorLogs = logEntries.Where(x => x.Level == LogLevel.Severe)
                        .Select(x => x.Message).ToList();

            return errorLogs;
        }
    }
}

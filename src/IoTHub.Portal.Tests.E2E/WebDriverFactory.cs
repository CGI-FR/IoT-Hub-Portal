// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E
{
    internal static class WebDriverFactory
    {
        private static IWebDriver? instance;

        public static IWebDriver Default
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--headless");
                options.AddArgument("--window-size=1920,1080");

                instance = new ChromeDriver(options);

                return instance;
            }
        }

        public static void Quit()
        {
            Default.Close();
            Default.Quit();

            instance?.Dispose();
            instance = null;
        }
    }
}

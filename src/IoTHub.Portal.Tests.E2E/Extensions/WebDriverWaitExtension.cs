// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E.Extensions
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    internal static class WebDriverExtension
    {
        public static bool WaitForAssertion(this IWebDriver driver, Func<IWebDriver, bool> expr, TimeSpan? timeout = null)
        {
            ArgumentNullException.ThrowIfNull(driver, nameof(driver));

            var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(1));

            return wait.Until(d => expr(driver));
        }
    }
}

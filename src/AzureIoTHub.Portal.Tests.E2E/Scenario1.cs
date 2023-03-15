// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.E2E
{
    using AzureIoTHub.Portal.Tests.E2E.Pages;
    using NUnit.Framework;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public class Scenario1
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--headless");
            options.AddArgument("--window-size=1920,1080");
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        [Test]
        public void TestScenario1()
        {
            var loginpage = new LoginPage(driver, wait);

            loginpage.Login();

            var model = new ModelPage(driver, wait);

            model.AddDeviceModel("Test Model AY", "Test Model Description");

            model.RemoveDeviceModel("Test Model AY");

            loginpage.Logout();
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}

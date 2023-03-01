// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.E2E
{
    using AzureIoTHub.Portal.Tests.E2E.Pages;
    using NUnit.Framework;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public class LoginTest
    {
        private IWebDriver driver;

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("headless");
            driver = new ChromeDriver("/usr/local/bin", options);
        }

        [Test]
        public void TestLogin()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            var loginpage = new LoginPage(driver, wait);

            loginpage.Login("achraf.boujida@etu.uca.fr", "dDpDDhSFL7VvrQA");

            _ = wait.Until(d => d.Url == "https://cgigeiotdemoportal.azurewebsites.net/");

            Assert.That(driver.Url, Is.EqualTo("https://cgigeiotdemoportal.azurewebsites.net/"));

            loginpage.Logout();

            System.Threading.Thread.Sleep(5000);

            Assert.That(driver.Url, Is.EqualTo("https://cgigeiotdemoportal.azurewebsites.net/authentication/logged-out"));
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}

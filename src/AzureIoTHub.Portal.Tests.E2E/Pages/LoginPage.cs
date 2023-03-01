// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.E2E.Pages
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public class LoginPage
    {
        public IWebDriver driver;
        public WebDriverWait wait;

        public LoginPage(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            driver.Manage().Window.Maximize();
            this.driver.Navigate().GoToUrl("https://cgigeiotdemoportal.azurewebsites.net");
            this.wait = wait;
        }

        public IWebElement UsernameField => driver.FindElement(By.Id("username"));
        public IWebElement PasswordField => driver.FindElement(By.Id("password"));
        public IWebElement LoginButton => driver.FindElement(By.Id("kc-login"));

        public void Login(string username, string password)
        {
            _ = wait.Until(d => d.FindElement(By.Id("kc-login")).Displayed);
            UsernameField.SendKeys(username);
            PasswordField.SendKeys(password);
            LoginButton.Click();
        }

        public void Logout()
        {
            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-menu-activator > .mud-button-root .mud-icon-root")).Displayed);
            driver.FindElement(By.CssSelector(".mud-menu-activator > .mud-button-root .mud-icon-root")).Click();
            driver.FindElement(By.CssSelector(".mud-list-item-icon")).Click();
        }
    }

}

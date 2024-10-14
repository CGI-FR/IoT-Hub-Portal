// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E.Pages
{
    public class LoginPage
    {
        private readonly WebDriverWait wait;
        private readonly IConfiguration configuration;

        public IWebElement UsernameField => WebDriverFactory.Default.FindElement(By.Id("username"));
        public IWebElement PasswordField => WebDriverFactory.Default.FindElement(By.Id("password"));
        public IWebElement LoginButton => WebDriverFactory.Default.FindElement(By.Id("kc-login"));

        public LoginPage(IConfiguration configuration)
        {
            this.configuration = configuration;

            var url = configuration["E2E_URL"];

            WebDriverFactory.Default.Navigate().GoToUrl(url);
            this.wait = new WebDriverWait(WebDriverFactory.Default, TimeSpan.FromSeconds(5));
        }

        public void Login()
        {
            _ = wait.Until(d => d.FindElement(By.Id("kc-login")).Displayed);

            UsernameField.SendKeys(configuration["E2E_USERNAME"]);
            PasswordField.SendKeys(configuration["E2E_PASSWORD"]);
            LoginButton.Click();

            _ = wait.Until(d => d.Url.StartsWith(this.configuration["URL"] ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        public void Logout()
        {
            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-menu-activator > .mud-button-root .mud-icon-root")).Displayed);
            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-menu-activator > .mud-button-root .mud-icon-root")).Click();
            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-list-item-icon")).Click();
        }
    }
}

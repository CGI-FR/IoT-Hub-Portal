// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.E2E.Pages
{
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public class ModelPage
    {
        public IWebDriver driver;
        public WebDriverWait wait;

        public ModelPage(IWebDriver driver, WebDriverWait wait)
        {
            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-paper:nth-child(2) .mud-nav-link-text")).Displayed);
            driver.FindElement(By.CssSelector(".mud-paper:nth-child(2) .mud-nav-link-text")).Click();
            this.driver = driver;
            this.wait = wait;
        }

        public void AddDeviceModel(string name, string description)
        {
            _ = wait.Until(d => d.FindElement(By.Id("addDeviceModelButton")).Displayed);

            driver.FindElement(By.Id("addDeviceModelButton")).Click();

            _ = wait.Until(d => d.FindElement(By.Id("Name")).Displayed);

            driver.FindElement(By.Id("Name")).Click();
            driver.FindElement(By.Id("Name")).SendKeys(name);
            driver.FindElement(By.Id("Description")).Click();
            driver.FindElement(By.Id("Description")).SendKeys(description);
            driver.FindElement(By.Id("SaveButton")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(driver.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device model successfully created."));

            driver.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);

        }

        public void SearchDeviceModel(string description)
        {
            _ = wait.Until(d => d.FindElement(By.ClassName("mud-expand-panel")).Displayed);

            driver.FindElement(By.ClassName("mud-expand-panel")).Click();

            _ = wait.Until(d => d.FindElement(By.Id("searchText")).Displayed);

            driver.FindElement(By.Id("searchText")).Click();
            driver.FindElement(By.Id("searchText")).SendKeys(description);

            driver.FindElement(By.CssSelector("button span.mud-button-label:nth-of-type(1)")).Click();
        }

        public void RemoveDeviceModel(string name)
        {
            SearchDeviceModel(name);

            var tableBody = driver.FindElement(By.CssSelector("tbody"));

            var rowsAreDisplayed = false;

            _ = wait.Until(d =>
            {
                var rows = tableBody.FindElements(By.CssSelector("tr"));
                rowsAreDisplayed = rows.All(row => row.Displayed && row.FindElements(By.CssSelector("td")).Count >= 2 && !string.IsNullOrEmpty(row.FindElements(By.CssSelector("td"))[1].Text));
                return rowsAreDisplayed;
            });

            var row = tableBody.FindElements(By.CssSelector("tr"))[0];

            var cell = row.FindElements(By.CssSelector("td"))[4];

            var buttonElement = cell.FindElement(By.Id("deleteButton"));
            var js = (IJavaScriptExecutor)driver;
            _ = js.ExecuteScript("arguments[0].click();", buttonElement);

            _ = wait.Until(d => d.FindElement(By.ClassName("outline-none")).Displayed);

            driver.FindElement(By.CssSelector(".mud-button-text-primary > .mud-button-label")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(driver.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device model " + name + " has been successfully deleted!"));

            driver.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);
        }
    }
}

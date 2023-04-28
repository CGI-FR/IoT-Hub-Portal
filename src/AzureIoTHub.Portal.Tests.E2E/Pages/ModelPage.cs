// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.E2E.Pages
{
    using FluentAssertions;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public class ModelPage
    {
        private readonly WebDriverWait wait;

        public ModelPage()
        {
            this.wait = new WebDriverWait(WebDriverFactory.Default, TimeSpan.FromSeconds(5));

            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-paper:nth-child(2) .mud-nav-link-text")).Displayed);
            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-paper:nth-child(2) .mud-nav-link-text")).Click();
        }

        public void AddDeviceModel(string name, string description)
        {

            _ = wait.Until(d => d.FindElement(By.Id("addDeviceModelButton")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("addDeviceModelButton")).Click();

            _ = wait.Until(d => d.FindElement(By.Id("Name")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("Name")).Click();
            WebDriverFactory.Default.FindElement(By.Id("Name")).SendKeys(name);
            WebDriverFactory.Default.FindElement(By.Id("Description")).Click();
            WebDriverFactory.Default.FindElement(By.Id("Description")).SendKeys(description);
            WebDriverFactory.Default.FindElement(By.Id("SaveButton")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(WebDriverFactory.Default.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device model successfully created."));

            WebDriverFactory.Default.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);

        }

        public void SearchDeviceModel(string description)
        {
            _ = wait.Until(d => d.FindElement(By.ClassName("mud-expand-panel")).Displayed);

            WebDriverFactory.Default.FindElement(By.ClassName("mud-expand-panel")).Click();

            _ = wait.Until(d => d.FindElement(By.Id("searchText")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("searchText")).Click();
            WebDriverFactory.Default.FindElement(By.Id("searchText")).SendKeys(description);

            WebDriverFactory.Default.FindElement(By.Id("searchButton")).Click();
        }

        public void RemoveDeviceModel(string name)
        {
            SearchDeviceModel(name);

            var tableBody = WebDriverFactory.Default.FindElement(By.CssSelector(".mud-table-root .mud-table-body"));

            _ = wait.Until(d =>
             {
                 try
                 {
                     _ = tableBody.FindElement(By.ClassName("mud-table-loading"));
                     return false;
                 }
                 catch (NoSuchElementException)
                 {
                     return true;
                 }
             });

            var rows = tableBody.FindElements(By.CssSelector("tr"));

            _ = rows.Count.Should().Be(1);

            var buttonElement = rows.First().FindElement(By.Id("deleteButton"));

            buttonElement.Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("outline-none")).Displayed);

            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-button-text-primary > .mud-button-label")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(WebDriverFactory.Default.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device model " + name + " has been successfully deleted!"));

            WebDriverFactory.Default.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);
        }
    }
}

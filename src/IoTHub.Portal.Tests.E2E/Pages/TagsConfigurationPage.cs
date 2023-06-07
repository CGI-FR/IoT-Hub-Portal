// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E.Pages
{
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public class TagsConfigurationPage
    {
        public WebDriverWait wait;

        public TagsConfigurationPage()
        {
            this.wait = new WebDriverWait(WebDriverFactory.Default, TimeSpan.FromSeconds(5));

            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-nav-group:nth-child(5) > .mud-collapse-container .mud-nav-link-text")).Displayed);
            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-nav-group:nth-child(5) > .mud-collapse-container .mud-nav-link-text")).Click();
        }

        public void NavigateToTagsPage()
        {
            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-nav-group:nth-child(5) > .mud-collapse-container .mud-nav-link-text")).Displayed);
            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-nav-group:nth-child(5) > .mud-collapse-container .mud-nav-link-text")).Click();
        }

        public void AddTag(string name, string label)
        {
            _ = wait.Until(d => d.FindElement(By.Id("addTagButton")).Displayed);
            WebDriverFactory.Default.FindElement(By.Id("addTagButton")).Click();

            WebDriverFactory.Default.FindElement(By.CssSelector(".tag- > .mud-table-cell:nth-child(1) .mud-input-slot:nth-child(1)")).Click();
            WebDriverFactory.Default.FindElement(By.CssSelector(".tag- > .mud-table-cell:nth-child(1) .mud-input-slot:nth-child(1)")).SendKeys(name);

            _ = wait.Until(d => d.FindElement(By.CssSelector(".tag- > .mud-table-cell:nth-child(2) .mud-input-slot:nth-child(1)")).Displayed);
            WebDriverFactory.Default.FindElement(By.CssSelector(".tag- > .mud-table-cell:nth-child(2) .mud-input-slot:nth-child(1)")).SendKeys(label);

            WebDriverFactory.Default.FindElement(By.CssSelector(".tag-" + name + " #saveButton .mud-icon-root")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);
            Assert.That(WebDriverFactory.Default.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Settings have been successfully updated!"));
            WebDriverFactory.Default.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();
            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);
        }

        public void RemoveTag(string name)
        {
            WebDriverFactory.Default.FindElement(By.CssSelector(".tag-" + name + " #deleteButton .mud-icon-root")).Click();
        }
    }
}

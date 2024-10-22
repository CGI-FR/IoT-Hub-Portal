// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E.Pages
{
    public class ConcentratorsPage
    {
        private readonly WebDriverWait wait;

        public ConcentratorsPage()
        {
            this.wait = new WebDriverWait(WebDriverFactory.Default, TimeSpan.FromSeconds(5));

            _ = wait.Until(d => d.FindElement(By.XPath("/html/body/div[2]/div[3]/aside/div/div/div[4]/div/div/div/div/div")).Displayed);
            WebDriverFactory.Default.FindElement(By.XPath("/html/body/div[2]/div[3]/aside/div/div/div[4]/div/div/div/div/div")).Click();
        }

        public void AddConcentrator(string id, string name)
        {
            _ = wait.Until(d => d.FindElement(By.Id("add-concentrator")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("add-concentrator")).Click();

            _ = wait.Until(d => d.FindElement(By.Id("DeviceId")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("DeviceId")).Click();
            WebDriverFactory.Default.FindElement(By.Id("DeviceId")).SendKeys(id);

            WebDriverFactory.Default.FindElement(By.Id("DeviceName")).Click();
            WebDriverFactory.Default.FindElement(By.Id("DeviceName")).SendKeys(name);

            WebDriverFactory.Default.FindElement(By.Id("LoraRegion")).Click();

            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-list.mud-list-padding li:first-child")).Displayed);
            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-list.mud-list-padding li:first-child")).Click();


            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(WebDriverFactory.Default.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device" + id + " has been successfully created! Please note that changes might take some minutes to be visible in the list..."));

            WebDriverFactory.Default.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);

        }

        public void SearchConcentrator(string id)
        {
            _ = wait.Until(d => d.FindElement(By.XPath("/html/body/div[2]/div[3]/aside/div/div/div[4]/div/div/div/div/div")).Displayed);

            WebDriverFactory.Default.FindElement(By.XPath("/html/body/div[2]/div[3]/aside/div/div/div[4]/div/div/div/div/div")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-expand-panel")).Displayed);

            WebDriverFactory.Default.FindElement(By.ClassName("mud-expand-panel-header")).Click();

            _ = wait.Until(d => d.FindElement(By.Id("searchKeyword")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("searchKeyword")).Click();
            WebDriverFactory.Default.FindElement(By.Id("searchKeyword")).SendKeys(id);

            WebDriverFactory.Default.FindElement(By.Id("searchButton")).Click();
        }

        public void RemoveConcentrator(string id)
        {
            SearchConcentrator(id);

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-tooltip-root mud-tooltip-inline")).Displayed);
            WebDriverFactory.Default.FindElement(By.ClassName("mud-tooltip-root mud-tooltip-inline")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("outline-none")).Displayed);

            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-button-text-primary > .mud-button-label")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(WebDriverFactory.Default.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device " + id + " A has been successfully deleted!"));


            WebDriverFactory.Default.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);
        }
    }
}

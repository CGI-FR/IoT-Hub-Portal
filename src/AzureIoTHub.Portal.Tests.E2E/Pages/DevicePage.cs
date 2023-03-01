// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.E2E.Pages
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public class DevicePage
    {
        public IWebDriver driver;
        public WebDriverWait wait;

        public DevicePage(IWebDriver driver, WebDriverWait wait)
        {
            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-paper:nth-child(1) .mud-nav-link-text")).Displayed);
            driver.FindElement(By.CssSelector(".mud-paper:nth-child(1) .mud-nav-link-text")).Click();
            this.driver = driver;
            this.wait = wait;
        }

        public void AddDevice(string id, string name, string model)
        {
            _ = wait.Until(d => d.FindElement(By.Id("addDeviceButton")).Displayed);
            driver.FindElement(By.Id("addDeviceButton")).Click();

            //_ = wait.Until(d => d.FindElement(By.CssSelector(".mud-input-adornment path:nth-child(2)")).Displayed);
            //driver.FindElement(By.CssSelector(".mud-input-adornment path:nth-child(2)")).Click();
            //driver.FindElement(By.CssSelector(".mud-typography-body2")).Click();
            _ = wait.Until(d => d.FindElement(By.Id("ModelId")).Displayed);
            driver.FindElement(By.Id("ModelId")).Click();
            driver.FindElement(By.Id("ModelId")).SendKeys(model);

            var inputElement = driver.FindElement(By.Id("DeviceID"));
            var js = (IJavaScriptExecutor)driver;
            _ = js.ExecuteScript("arguments[0].click();", inputElement);
            driver.FindElement(By.Id("DeviceID")).SendKeys(id);

            inputElement = driver.FindElement(By.Id("DeviceName"));
            _ = js.ExecuteScript("arguments[0].click();", inputElement);
            driver.FindElement(By.Id("DeviceName")).SendKeys(name);

            inputElement = driver.FindElement(By.Id("SaveButton"));
            _ = js.ExecuteScript("arguments[0].click();", inputElement);
        }

        public void SearchDevice(string id, string model = null)
        {
            _ = wait.Until(d => d.FindElement(By.ClassName("mud-expand-panel")).Displayed);

            driver.FindElement(By.ClassName("mud-expand-panel")).Click();

            _ = wait.Until(d => d.FindElement(By.Id("searchID")).Displayed);

            driver.FindElement(By.Id("searchID")).Click();
            driver.FindElement(By.Id("searchID")).SendKeys(id);
            driver.FindElement(By.Id("ModelId")).Click();
            driver.FindElement(By.Id("ModelId")).SendKeys(model);

            driver.FindElement(By.Id("searchButton")).Click();
        }

        public void RemoveDevice(string id, string model = null)
        {
            SearchDevice(id, model);

            _ = wait.Until(d => d.FindElement(By.CssSelector("#delete-device-" + id + " path:nth-child(2)")).Displayed);

            // deleting
            driver.FindElement(By.CssSelector("#delete-device-" + id + " path:nth-child(2)")).Click();
            driver.FindElement(By.CssSelector("#delete-device > .mud-button-label")).Click();
        }
    }
}

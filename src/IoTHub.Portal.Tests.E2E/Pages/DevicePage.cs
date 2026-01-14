// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E.Pages
{
    public class DevicePage
    {
#pragma warning disable CA1051 // Ne pas déclarer de champs d'instances visibles
        public IWebDriver Driver;
        public WebDriverWait Wait;
#pragma warning restore CA1051 // Ne pas déclarer de champs d'instances visibles

        public DevicePage(IWebDriver driver, WebDriverWait wait)
        {
            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-paper:nth-child(1) .mud-nav-link-text")).Displayed);
            driver.FindElement(By.CssSelector(".mud-paper:nth-child(1) .mud-nav-link-text")).Click();
            this.Driver = driver;
            this.Wait = wait;
        }

        public void AddDevice(string id, string name, string model)
        {
            _ = this.Wait.Until(d => d.FindElement(By.Id("addDeviceButton")).Displayed);
            this.Driver.FindElement(By.Id("addDeviceButton")).Click();

            //_ = wait.Until(d => d.FindElement(By.CssSelector(".mud-input-adornment path:nth-child(2)")).Displayed);
            //driver.FindElement(By.CssSelector(".mud-input-adornment path:nth-child(2)")).Click();
            //driver.FindElement(By.CssSelector(".mud-typography-body2")).Click();
            _ = this.Wait.Until(d => d.FindElement(By.Id("ModelId")).Displayed);
            this.Driver.FindElement(By.Id("ModelId")).Click();
            this.Driver.FindElement(By.Id("ModelId")).SendKeys(model);

            var inputElement = this.Driver.FindElement(By.Id("DeviceID"));
            var js = (IJavaScriptExecutor)this.Driver;
            _ = js.ExecuteScript("arguments[0].click();", inputElement);
            this.Driver.FindElement(By.Id("DeviceID")).SendKeys(id);

            inputElement = this.Driver.FindElement(By.Id("DeviceName"));
            _ = js.ExecuteScript("arguments[0].click();", inputElement);
            this.Driver.FindElement(By.Id("DeviceName")).SendKeys(name);

            inputElement = this.Driver.FindElement(By.Id("SaveButton"));
            _ = js.ExecuteScript("arguments[0].click();", inputElement);
        }

        public void SearchDevice(string id, string? model = null)
        {
            _ = this.Wait.Until(d => d.FindElement(By.ClassName("mud-expand-panel")).Displayed);

            this.Driver.FindElement(By.ClassName("mud-expand-panel")).Click();

            _ = this.Wait.Until(d => d.FindElement(By.Id("searchID")).Displayed);

            this.Driver.FindElement(By.Id("searchID")).Click();
            this.Driver.FindElement(By.Id("searchID")).SendKeys(id);
            this.Driver.FindElement(By.Id("ModelId")).Click();
            this.Driver.FindElement(By.Id("ModelId")).SendKeys(model);

            this.Driver.FindElement(By.Id("searchButton")).Click();
        }

        public void RemoveDevice(string id, string? model = null)
        {
            SearchDevice(id, model);

            _ = this.Wait.Until(d => d.FindElement(By.CssSelector("#delete-device-" + id + " path:nth-child(2)")).Displayed);

            // deleting
            this.Driver.FindElement(By.CssSelector("#delete-device-" + id + " path:nth-child(2)")).Click();
            this.Driver.FindElement(By.CssSelector("#delete-device > .mud-button-label")).Click();
        }
    }
}

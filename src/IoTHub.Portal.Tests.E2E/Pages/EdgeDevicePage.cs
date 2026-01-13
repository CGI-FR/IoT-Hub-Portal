// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E.Pages
{
    public class EdgeDevicePage
    {

        private readonly WebDriverWait wait;

        public EdgeDevicePage()
        {
            this.wait = new WebDriverWait(WebDriverFactory.Default, TimeSpan.FromSeconds(5));

            _ = wait.Until(d => d.FindElement(By.XPath("/ html / body / div[2] / div[3] / aside / div / div / div[3] / div / div / div / div / div[1] / a")).Displayed);

            WebDriverFactory.Default.FindElement(By.XPath("/ html / body / div[2] / div[3] / aside / div / div / div[3] / div / div / div / div / div[1] / a")).Click();
        }




        public void AddEdgeDevice(string id, string name, string model)
        {
            _ = wait.Until(d => d.FindElement(By.XPath("/html/body/div[2]/div[3]/div/div/div/div[2]/div/div[1]/div[3]/button")).Displayed);

            WebDriverFactory.Default.FindElement(By.XPath("/html/body/div[2]/div[3]/div/div/div/div[2]/div/div[1]/div[3]/button")).Click();
            _ = wait.Until(d => d.FindElement(By.Id("ModelId")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("ModelId")).Click();

            Thread.Sleep(2000);

            WebDriverFactory.Default.FindElement(By.Id("ModelId")).SendKeys(model + Keys.Enter);

            Thread.Sleep(2000);

            // driver.FindElement(By.Id("ModelId")).SendKeys(Keys.Enter);

            WebDriverFactory.Default.FindElement(By.Id("DeviceId")).Click();
            WebDriverFactory.Default.FindElement(By.Id("DeviceId")).SendKeys(id);
            WebDriverFactory.Default.FindElement(By.Id("DeviceName")).Click();
            WebDriverFactory.Default.FindElement(By.Id("DeviceName")).SendKeys(name);


            Thread.Sleep(2000);


            // Find the "Save" button
            var saveButton = WebDriverFactory.Default.FindElement(By.XPath("//span[@class='mud-button-label' and text()='Save']"));

            // Scroll to the "Save" button
            //((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", saveButton);

            // Click the element
            saveButton.Click();

            Thread.Sleep(5000);

            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(WebDriverFactory.Default.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device " + id + " has been successfully created!"));

            WebDriverFactory.Default.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);
        }

        public void SearchEdgeDevice(string name)
        {
            _ = wait.Until(d => d.FindElement(By.ClassName("mud-expand-panel-header")).Displayed);

            WebDriverFactory.Default.FindElement(By.ClassName("mud-expand-panel-header")).Click();

            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-input-input-control .mud-input-slot:nth-child(1)")).Displayed);

            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-input-input-control .mud-input-slot:nth-child(1)")).Click();
            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-input-input-control .mud-input-slot:nth-child(1)")).SendKeys(name);


            WebDriverFactory.Default.FindElement(By.Id("searchFilterButton")).Click();

        }

        public void RemoveEdgeDevice(string id)
        {
            SearchEdgeDevice(id);

            _ = wait.Until(d => d.FindElement(By.CssSelector("#delete_789456 path:nth-child(2)")).Displayed);
            WebDriverFactory.Default.FindElement(By.CssSelector("#delete_789456 path:nth-child(2)")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("outline-none")).Displayed);

            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-button-text-primary > .mud-button-label")).Click();
            Thread.Sleep(5000);


            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(WebDriverFactory.Default.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device " + id + " has been successfully deleted!"));

            WebDriverFactory.Default.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);
        }
    }
}

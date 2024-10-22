// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E.Pages
{
    public class EdgeModelPage
    {
        private readonly WebDriverWait wait;

        public EdgeModelPage()
        {
            this.wait = new WebDriverWait(WebDriverFactory.Default, TimeSpan.FromSeconds(5));

            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-navmenu > .mud-nav-item:nth-child(2) .mud-nav-link-text")).Displayed);

            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-navmenu > .mud-nav-item:nth-child(2) .mud-nav-link-text")).Click();
        }

        public void NavigateToEdgeModelPage()
        {
            _ = wait.Until(d => d.FindElement(By.CssSelector(".mud-navmenu > .mud-nav-item:nth-child(2) .mud-nav-link-text")).Displayed);


            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-navmenu > .mud-nav-item:nth-child(2) .mud-nav-link-text")).Click();
        }


        public void AddEdgeModel(string name, string description)
        {
            _ = wait.Until(d => d.FindElement(By.Id("addEdgeModelButton")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("addEdgeModelButton")).Click();

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

        public void SearchEdgeModel(string description)
        {
            WebDriverFactory.Default.FindElement(By.ClassName("mud-expand-panel-text")).Click();

            _ = wait.Until(d => d.FindElement(By.Id("edge-model-search-keyword")).Displayed);


            WebDriverFactory.Default.FindElement(By.Id("edge-model-search-keyword")).SendKeys(description);

            _ = wait.Until(d => d.FindElement(By.Id("edge-model-search-button")).Displayed);

            WebDriverFactory.Default.FindElement(By.Id("edge-model-search-button")).Click();

            _ = wait.Until(d =>
            {
                try
                {
                    _ = WebDriverFactory.Default.FindElement(By.ClassName("mud-table-loading"));
                    return false;
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
            });
        }


        public void RemoveEdgeModel(string name)
        {
            SearchEdgeModel(name);

            WebDriverFactory.Default.FindElement(By.Id("deleteButton")).Click();

            _ = wait.Until(d => d.FindElement(By.ClassName("outline-none")).Displayed);

            WebDriverFactory.Default.FindElement(By.CssSelector(".mud-button-text-primary > .mud-button-label")).Click();
            System.Threading.Thread.Sleep(5000);


            _ = wait.Until(d => d.FindElement(By.ClassName("mud-snackbar-content-message")).Displayed);

            Assert.That(WebDriverFactory.Default.FindElement(By.ClassName("mud-snackbar-content-message")).Text, Is.EqualTo("Device model " + name + " has been successfully deleted!"));

            WebDriverFactory.Default.FindElement(By.CssSelector("button[class='mud-button-root mud-icon-button mud-ripple mud-ripple-icon mud-icon-button-size-small ms-2']")).Click();

            _ = wait.Until(d => !d.FindElement(By.Id("mud-snackbar-container")).Displayed);
        }


    }
}

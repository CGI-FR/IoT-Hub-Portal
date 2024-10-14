// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E
{
    public class DeviceModels : E2ETest
    {
        private LoginPage loginPage;

        [SetUp]
        public void SetUp()
        {
            loginPage = new LoginPage(Configuration);

            loginPage.Login();
        }

        [TearDown]
        public override void TearDown()
        {
            loginPage.Logout();

            base.TearDown();
        }

        [Test]
        public void UserCanAddAndRemoveDeviceModel()
        {
            var modelName = Fixture.Create<string>();

            var model = new ModelPage();

            model.AddDeviceModel(modelName, "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Vel fringilla est ullamcorper eget nulla. Vel turpis nunc eget lorem dolor sed viverra ipsum nunc. Eget dolor morbi non arcu. Urna duis convallis convallis tellus id interdum velit laoreet id. ");

            model.RemoveDeviceModel(modelName);
        }
    }
}

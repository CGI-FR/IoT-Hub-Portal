// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E
{
    public class DeviceTags : E2ETest
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
        public void UserCanAddAndRemoveTagsConfiguration()
        {
            var fixture = new Fixture();

            var tagName = new string(
                fixture.CreateMany<char>(4)
                    .Where(c => char.IsLetterOrDigit(c))
                    .ToArray()
            );

            var tagLabel = new string(
                fixture.CreateMany<char>(4)
                    .Where(c => char.IsLetterOrDigit(c))
                    .ToArray()
            );

            var tag = new TagsConfigurationPage();

            tag.AddTag(tagName, tagLabel);

            tag.RemoveTag(tagName);
        }
    }
}

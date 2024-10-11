// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.E2E
{
    public class Authentication : E2ETest
    {
        [Test]
        public void UserCanLoginLogout()
        {
            var loginpage = new LoginPage(Configuration);

            loginpage.Login();

            loginpage.Logout();
        }
    }
}

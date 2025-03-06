// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Helpers
{
    [TestFixture]
    public class DatabaseHelperTests
    {
        [Test]
        public void GetMySqlServerVersion_InvalidConnectionString_ReturnsDefaultMySqlServerVersion()
        {
            // Arrange
            var expectedsqlserverversion = new MySqlServerVersion(new Version(8, 0, 32));
            var mysqlConnectionString = Guid.NewGuid().ToString();

            // Act
            var result = DatabaseHelper.GetMySqlServerVersion(mysqlConnectionString);

            // Assert
            Assert.AreEqual(expectedsqlserverversion, result);
        }
    }
}

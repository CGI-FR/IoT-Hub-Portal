// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Mappers
{
    using Azure.Data.Tables;
    using Models.v10;
    using AzureIoTHub.Portal.Server.Mappers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceTagMapperTests
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private static DeviceTagMapper CreateDeviceTagMapper()
        {
            return new DeviceTagMapper();
        }

        [Test]
        public void GetDeviceTagStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceTagMapper = CreateDeviceTagMapper();
            var entity = new TableEntity
            {
                RowKey = "ExpectedRowKey"
            };

            entity["Label"] = "ExpectedLabel";
            entity["Required"] = false;
            entity["Searchable"] = false;

            // Act
            var result = deviceTagMapper.GetDeviceTag(entity);

            // Assert
            Assert.AreEqual("ExpectedRowKey", result.Name);
            Assert.AreEqual("ExpectedLabel", result.Label);
            Assert.IsFalse(result.Required);
            Assert.IsFalse(result.Searchable);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTableEntityStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceTagMapper = CreateDeviceTagMapper();
            var entity = new TableEntity();

            var element = new DeviceTagDto
            {
                Name = "ExpectedName",
                Label = "ExpectedLabel",
                Required = true,
                Searchable = true
            };

            // Act
            deviceTagMapper.UpdateTableEntity(entity, element);

            // Assert
            Assert.AreEqual("ExpectedLabel", entity["Label"]);
            Assert.IsTrue(bool.Parse(entity["Required"].ToString()));
            Assert.IsTrue(bool.Parse(entity["Searchable"].ToString()));
            this.mockRepository.VerifyAll();
        }
    }
}

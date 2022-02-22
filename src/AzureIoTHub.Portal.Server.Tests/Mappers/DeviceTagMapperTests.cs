using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.v10.Device;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Mappers
{
    [TestFixture]
    public class DeviceTagMapperTests
    {
        private MockRepository mockRepository;
        
        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private DeviceTagMapper CreateDeviceTagMapper()
        {
            return new DeviceTagMapper();
        }
        
        [Test]
        public void GetDeviceTag_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceTagMapper = this.CreateDeviceTagMapper();
            var entity = new TableEntity();

            entity.RowKey = "ExpectedRowKey";
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
        public void UpdateTableEntity_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceTagMapper = this.CreateDeviceTagMapper();
            var entity = new TableEntity();

            var element = new DeviceTag
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

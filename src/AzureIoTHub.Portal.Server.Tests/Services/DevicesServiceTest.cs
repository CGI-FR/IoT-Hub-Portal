using AzureIoTHub.Portal.Server.Interfaces;
using Moq;
using NUnit.Framework;

namespace AzureIoTHub.Portal.Server.Tests.Services
{
    internal class DevicesServiceTest
    {

        [Test]
        public void GetAllEdgeDevice_returnValue()
        {
            // Arrange
            var moqService = new Mock<IDevicesService>();
        }
    }
}

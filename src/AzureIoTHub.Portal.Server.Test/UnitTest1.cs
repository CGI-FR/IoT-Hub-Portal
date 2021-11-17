using AzureIoTHub.Portal.Server.Controllers;
using NUnit.Framework;

namespace AzureIoTHub.Portal.Server.Test
{
    public class Tests
    {
        [Test]
        public void AddTest()
        {
            //
            var math = new Math();

            //
            var result = math.Add(1, 2);

            // Assert
            Assert.AreEqual(3, result);
        }
    }
}
// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0.LoRaWAN
{
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;
    using UnitTests.Bases;
    using Microsoft.AspNetCore.Mvc;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaWANFrequencyPlansControllerTests : BackendUnitTest
    {
        private static LoRaWANFrequencyPlansController CreateLoRaWANFrequencyPlansController()
        {
            return new LoRaWANFrequencyPlansController();
        }

        [Test]
        public void GetFrequencyPlans()
        {
            // Arrange
            var loRaWANFrequencyPlansController = CreateLoRaWANFrequencyPlansController();

            // Act
            var result = loRaWANFrequencyPlansController.GetFrequencyPlans();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<ActionResult<IEnumerable<FrequencyPlan>>>(result);
            base.MockRepository.VerifyAll();
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Filters
{
    using RouteData = Microsoft.AspNetCore.Routing.RouteData;

    [TestFixture]
    public class LoRaFeatureActiveFilterAttributeTest
    {
        private MockRepository mockRepository;
        private Mock<ConfigHandler> mockConfigHandler;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            // this.mockActionExecutingContext = this.mockRepository.Create<ActionExecutingContext>();
            this.mockConfigHandler = this.mockRepository.Create<ConfigHandler>();
        }

        private static LoRaFeatureActiveFilterAttribute CreateAttributeFilter()
        {
            return new LoRaFeatureActiveFilterAttribute();
        }

        [Test]
        public void WhenLoRaIsDisabledShouldReturnHttp400()
        {
            // Arrange
            _ = this.mockConfigHandler
                .SetupGet(c => c.IsLoRaEnabled)
                .Returns(false);

            var serviceProviderMock = new Mock<IServiceProvider>();
            _ = serviceProviderMock.Setup(provider => provider.GetService(typeof(ConfigHandler)))
                .Returns(this.mockConfigHandler.Object);

            var httpContextMock = new Mock<HttpContext>();
            _ = httpContextMock.SetupGet(context => context.RequestServices)
                .Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext(
                httpContextMock.Object,
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>());

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                Mock.Of<Controller>());

            var actionFilter = CreateAttributeFilter();

            // Act
            actionFilter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsAssignableFrom<BadRequestObjectResult>(actionExecutingContext.Result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenLoRaIsEnabledShouldReturnNull()
        {
            // Arrange
            _ = this.mockConfigHandler
                .SetupGet(c => c.IsLoRaEnabled)
                .Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            _ = serviceProviderMock.Setup(provider => provider.GetService(typeof(ConfigHandler)))
                .Returns(this.mockConfigHandler.Object);

            var httpContextMock = new Mock<HttpContext>();
            _ = httpContextMock.SetupGet(context => context.RequestServices)
                .Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext(
                httpContextMock.Object,
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>());

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                Mock.Of<Controller>());

            var actionFilter = CreateAttributeFilter();

            // Act
            actionFilter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsNull(actionExecutingContext.Result);
            this.mockRepository.VerifyAll();
        }
    }
}

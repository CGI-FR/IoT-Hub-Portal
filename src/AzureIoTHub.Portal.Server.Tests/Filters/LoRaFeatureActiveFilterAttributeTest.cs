using AzureIoTHub.Portal.Server.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AzureIoTHub.Portal.Server.Startup;

namespace AzureIoTHub.Portal.Server.Tests.Filters
{
    [TestFixture]
    public class LoRaFeatureActiveFilterAttributeTest
    {
        private MockRepository mockRepository;
        // private Mock<ActionExecutingContext> mockActionExecutingContext;
        private Mock<ConfigHandler> mockConfigHandler;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            // this.mockActionExecutingContext = this.mockRepository.Create<ActionExecutingContext>();
            this.mockConfigHandler = this.mockRepository.Create<ConfigHandler>();
        }

        private LoRaFeatureActiveFilterAttribute CreateAttributeFilter()
        {
            return new LoRaFeatureActiveFilterAttribute();
        }

        [Test]
        public void When_LoRa_Is_Disabled_Should_Return_Http_400()
        {
            // Arrange
            this.mockConfigHandler
                .SetupGet(c => c.IsLoRaEnabled)
                .Returns(false);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(provider => provider.GetService(typeof(ConfigHandler)))
                .Returns(this.mockConfigHandler.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(context => context.RequestServices)
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

            var actionFilter = this.CreateAttributeFilter();

            // Act
            actionFilter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsAssignableFrom<BadRequestObjectResult>(actionExecutingContext.Result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void When_LoRa_Is_Enabled_Should_Return_Null()
        {
            // Arrange
            this.mockConfigHandler
                .SetupGet(c => c.IsLoRaEnabled)
                .Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(provider => provider.GetService(typeof(ConfigHandler)))
                .Returns(this.mockConfigHandler.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(context => context.RequestServices)
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

            var actionFilter = this.CreateAttributeFilter();

            // Act
            actionFilter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsNull(actionExecutingContext.Result);
            this.mockRepository.VerifyAll();
        }
    }
}

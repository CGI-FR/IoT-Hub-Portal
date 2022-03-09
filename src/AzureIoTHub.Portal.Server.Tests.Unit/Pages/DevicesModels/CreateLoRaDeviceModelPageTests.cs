using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Interop;
using MudBlazor.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DevicesModels
{
    [TestFixture]
    public class CreateLoRaDeviceModelPageTests
    {
        private Bunit.TestContext testContext;
        private string mockModelId = Guid.NewGuid().ToString();

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;
        private MockHttpMessageHandler mockHttpClient;

        private string apiSettingsLora = "/api/settings/lora";
        private string apiBaseUrl => $"/api/models";
        private string lorawanApiBaseUrl => $"/api/lorawan/models";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            this.mockHttpClient = testContext.Services
                                            .AddMockHttpClient();

            testContext.Services.AddSingleton(this.mockDialogService.Object);

            testContext.Services.AddMudServices();

            testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

            mockNavigationManager = testContext.Services.GetRequiredService<FakeNavigationManager>();

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void Click_On_Add_Command_Should_Add_New_Command()
        {

        }
    }
}

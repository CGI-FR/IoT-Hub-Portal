// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DevicesModels
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateLoRaDeviceModelPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private readonly string mockModelId = Guid.NewGuid().ToString();

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;
        private MockHttpMessageHandler mockHttpClient;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            this.mockHttpClient = testContext.Services
                                            .AddMockHttpClient();

            _ = testContext.Services.AddSingleton(this.mockDialogService.Object);

            _ = testContext.Services.AddMudServices();

            _ = testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            _ = testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

            mockNavigationManager = testContext.Services.GetRequiredService<FakeNavigationManager>();

            this.mockHttpClient.AutoFlush = true;
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

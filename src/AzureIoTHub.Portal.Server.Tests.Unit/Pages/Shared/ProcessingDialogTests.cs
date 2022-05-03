// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Shared;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class ProcessingDialogTests : IDisposable
    {
        private Bunit.TestContext testContext;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();
            testContext.AddTestServices();
        }

        [Test]
        public async Task DisplayDialog()
        {
            // Assert
            var comp = this.testContext.RenderComponent<MudDialogProvider>();
            _ = comp.Markup.Trim().Should().BeEmpty();
            var service = this.testContext.Services.GetService<IDialogService>() as DialogService;
            _ = service.Should().NotBe(null);
            IDialogReference dialogReference = null;

            // Opens dialog
            var parameters = new DialogParameters{{ "ContentText", "Processing" } };
            await comp.InvokeAsync(() => dialogReference = service.Show<ProcessingDialog>("Processing", parameters));
            _ = dialogReference.Should().NotBe(null);

            // Checks dialog content
            _ = comp.Find("div.mud-dialog-container").Should().NotBe(null);
            _ = comp.Find("p.mud-typography").InnerHtml.Should().Be("Processing");
            _ = comp.Find("#contentText").InnerHtml.Should().Be("Processing");

            // Dialog should not be closable through backdrop click
            comp.Find("div.mud-overlay").Click();
            comp.WaitForAssertion(() => comp.Markup.Trim().Should().NotBeEmpty(), TimeSpan.FromSeconds(5));

            // Dialog should be closable through a method
            await comp.InvokeAsync(() => service.Close(dialogReference as DialogReference));
            var result = await dialogReference.Result;
            _ = result.Cancelled.Should().BeFalse();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
    public static class TestContextExtensions
    {
        public static void AddTestServices(this Bunit.TestContext ctx)
        {
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;
            _ = ctx.Services.AddSingleton<IDialogService, DialogService>();
            _ = ctx.Services.AddMudServices();
        }
    }
}

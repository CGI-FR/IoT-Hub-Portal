// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bunit;
    using Client.Extensions;
    using Client.Models;
    using Client.Shared;
    using FluentAssertions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class ProblemDetailsDialogTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockRepository mockRepository;
        private Mock<IWebAssemblyHostEnvironment> mockWebAssemblyHostEnvironment;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockWebAssemblyHostEnvironment = this.mockRepository.Create<IWebAssemblyHostEnvironment>();

            _ = this.testContext.Services.AddSingleton(this.mockWebAssemblyHostEnvironment.Object);

            _ = this.testContext.Services.AddMudServices();
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public async Task ProblemDetailsDialogShouldBeRenderedWithProblemDetailsInfosAsync()
        {
            // Arrange
            var problemDetailsWithExceptionDetails = new ProblemDetailsWithExceptionDetails
            {
                Title = "title",
                Detail = "detail",
                Status = 400,
                TraceId = "traceId",
                ExceptionDetails = new List<ProblemDetailsWithExceptionDetails.ExceptionDetail>()
            };

            _ = this.mockWebAssemblyHostEnvironment.Setup(c => c.Environment)
                .Returns("Development");

            var cut = RenderComponent<MudDialogProvider>();
            var service = this.testContext.Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "ProblemDetails", problemDetailsWithExceptionDetails
                }
            };

            // Act
            await cut.InvokeAsync(() => service?.Show<ProblemDetailsDialog>(string.Empty, parameters));

            // Assert
            var mudListItems = cut.FindComponents<MudListItem>();
            _ = mudListItems.Count.Should().Be(3);
            _ = mudListItems.Count(component => component.Markup
                .Contains($"Status: {problemDetailsWithExceptionDetails.Status}", StringComparison.OrdinalIgnoreCase)).Should().Be(1);
            _ = mudListItems.Count(component => component.Markup
                .Contains($"Detail: {problemDetailsWithExceptionDetails.Detail}", StringComparison.OrdinalIgnoreCase)).Should().Be(1);
            _ = mudListItems.Count(component => component.Markup
                .Contains($"TraceId: {problemDetailsWithExceptionDetails.TraceId}", StringComparison.OrdinalIgnoreCase)).Should().Be(1);

            var mudExpansionPanel = cut.FindComponent<MudExpansionPanel>();
            mudExpansionPanel.Instance.Expand();

            var exceptionDetailsMudTextField = mudExpansionPanel.FindComponent<MudTextField<string>>();
            _ = exceptionDetailsMudTextField.Instance.Value.Should().Be(problemDetailsWithExceptionDetails.ToJson());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.testContext?.Dispose();
        }
    }
}

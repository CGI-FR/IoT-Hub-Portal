// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using System;
    using Bunit;
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
        private Mock<IDialogService> mockDialogService;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockWebAssemblyHostEnvironment = this.mockRepository.Create<IWebAssemblyHostEnvironment>();
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            _ = this.testContext.Services.AddSingleton(this.mockWebAssemblyHostEnvironment.Object);
            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);

            _ = this.testContext.Services.AddMudServices();
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void ProblemDetailsDialogShouldBeRenderedWithProblemDetailsInfos()
        {
            // Arrange
            var problemDetailsWithExceptionDetails = new ProblemDetailsWithExceptionDetails
            {
                Title = "title",
                Detail = "detail",
                Status = 400,
                TraceId = "traceId",
            };

            _ = this.mockWebAssemblyHostEnvironment.Setup(c => c.Environment)
                .Returns("Development");

            // Act
            var cut = RenderComponent<ProblemDetailsDialog>(ComponentParameter.CreateParameter("ProblemDetails", problemDetailsWithExceptionDetails));

            // Assert
            _ = cut.Instance.ProblemDetails.Should().BeEquivalentTo(problemDetailsWithExceptionDetails);
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

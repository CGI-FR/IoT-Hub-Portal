// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using System;
    using System.Collections.Generic;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Shared;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class ErrorTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockRepository mockRepository;
        private Mock<ISnackbar> mockSnackbar;
        private Mock<IWebAssemblyHostEnvironment> mockWebAssemblyHostEnvironment;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockSnackbar = new Mock<ISnackbar>();
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
        public void ProcessProblemDetailsShouldAddSnackBar()
        {
            // Arrange
            var problemDetailsException = new ProblemDetailsException(new ProblemDetailsWithExceptionDetails
            {
                Title = "title",
                Detail = "detail",
                Status = 400,
                TraceId = "traceId",
                ExceptionDetails = new List<ProblemDetailsWithExceptionDetails.ExceptionDetail>()
            });

            _ = this.mockSnackbar.Setup(c => c.Add(It.Is<string>(x => x == problemDetailsException.ProblemDetailsWithExceptionDetails.Detail),
                It.Is<Severity>(severity => severity == Severity.Error), It.IsAny<Action<SnackbarOptions>>()));

            var cut = RenderComponent<Error>();

            // Act
            cut.Instance.ProcessProblemDetails(problemDetailsException);

            // Assert
            this.mockRepository.VerifyAll();
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

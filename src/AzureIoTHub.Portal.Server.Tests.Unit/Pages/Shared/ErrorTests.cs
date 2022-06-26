// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using System;
    using System.Collections.Generic;
    using Client.Exceptions;
    using Client.Models;
    using Client.Shared;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class ErrorTests : BlazorUnitTest
    {
        private Mock<ISnackbar> mockSnackBar;
        private Mock<IWebAssemblyHostEnvironment> mockWebAssemblyHostEnvironment;

        public override void Setup()
        {
            base.Setup();

            this.mockSnackBar = MockRepository.Create<ISnackbar>();
            this.mockWebAssemblyHostEnvironment = MockRepository.Create<IWebAssemblyHostEnvironment>();

            _ = Services.AddSingleton(this.mockWebAssemblyHostEnvironment.Object);
            _ = Services.AddSingleton(this.mockSnackBar.Object);
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

            _ = this.mockSnackBar.Setup(c => c.Add(It.Is<string>(x => x == problemDetailsException.ProblemDetailsWithExceptionDetails.Detail),
                It.Is<Severity>(severity => severity == Severity.Error), It.IsAny<Action<SnackbarOptions>>()))
                .Returns(this.mockSnackBar.Object as Snackbar);

            var cut = RenderComponent<Error>();

            // Act
            cut.Instance.ProcessProblemDetails(problemDetailsException);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}

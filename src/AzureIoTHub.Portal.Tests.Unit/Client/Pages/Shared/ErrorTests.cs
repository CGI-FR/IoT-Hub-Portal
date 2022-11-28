// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Shared
{
    using System.Collections.Generic;
    using System.Linq;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Shared;
    using UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class ErrorTests : BlazorUnitTest
    {
        [Test]
        public void ProcessProblemDetailsShouldSnackBar()
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

            var snackBarService = Services.GetRequiredService<ISnackbar>();

            var cut = RenderComponent<Error>();

            // Act
            cut.Instance.ProcessProblemDetails(problemDetailsException);

            // Assert
            var snackBars = snackBarService.ShownSnackbars.ToList();

            _ = snackBars.Count.Should().Be(1);

            var errorSnackBar = snackBars.First();

            _ = errorSnackBar.Severity.Should().Be(Severity.Error);
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Shared
{
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

        [Test]
        public void ProcessProblemDetails_UnauthorizedHttpStatusCode_SnackbarItemIsNotAdded()
        {
            // Arrange
            var problemDetailsException = new ProblemDetailsException(new ProblemDetailsWithExceptionDetails
            {
                Title = "title",
                Detail = "detail",
                Status = 401,
                TraceId = "traceId",
                ExceptionDetails = new List<ProblemDetailsWithExceptionDetails.ExceptionDetail>()
            });

            var snackBarService = Services.GetRequiredService<ISnackbar>();

            var cut = RenderComponent<Error>();

            // Act
            cut.Instance.ProcessProblemDetails(problemDetailsException);

            // Assert
            var snackBars = snackBarService.ShownSnackbars.ToList();

            _ = snackBars.Count.Should().Be(0);
        }
    }
}

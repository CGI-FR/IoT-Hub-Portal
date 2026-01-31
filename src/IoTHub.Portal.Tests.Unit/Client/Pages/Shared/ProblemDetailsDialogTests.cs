// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Shared
{
    [TestFixture]
    public class ProblemDetailsDialogTests : BlazorUnitTest
    {
        private Mock<IWebAssemblyHostEnvironment> mockWebAssemblyHostEnvironment;

        public override void Setup()
        {
            base.Setup();

            this.mockWebAssemblyHostEnvironment = MockRepository.Create<IWebAssemblyHostEnvironment>();
            _ = Services.AddSingleton(this.mockWebAssemblyHostEnvironment.Object);
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
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "ProblemDetails", problemDetailsWithExceptionDetails
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<ProblemDetailsDialog>(string.Empty, parameters));

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
    }
}

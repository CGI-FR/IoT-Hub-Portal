// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Ideas
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Dialogs.Ideas;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class SubmitIdeaDialogTests : BlazorUnitTest
    {
        private Mock<IIdeaClientService> mockIdeaClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockIdeaClientService = MockRepository.Create<IIdeaClientService>();

            _ = Services.AddSingleton(this.mockIdeaClientService.Object);
        }

        [Test]
        public async Task SubmitIdeaDialogMustBeRenderedOnShow()
        {
            // Arrange
            var cut = RenderComponent<MudDialogProvider>();
            var dialogService = Services.GetService<IDialogService>() as DialogService;

            // Act
            _ = await cut.InvokeAsync(() => dialogService?.Show<SubmitIdeaDialog>(string.Empty));

            // Assert
            cut.WaitForAssertion(() => cut.Find("#idea-form").Should().NotBeNull());
            cut.WaitForAssertion(() => cut.Find("#idea-title").Should().NotBeNull());
            cut.WaitForAssertion(() => cut.Find("#idea-description").Should().NotBeNull());
            cut.WaitForAssertion(() => cut.Find("#idea-cancel").Should().NotBeNull());
            cut.WaitForAssertion(() => cut.Find("#idea-submit").Should().NotBeNull());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task SubmitIdeaDialogMustBeCancelledOnClickOnCancel()
        {
            // Arrange
            var cut = RenderComponent<MudDialogProvider>();
            var dialogService = Services.GetService<IDialogService>() as DialogService;

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = dialogService?.Show<SubmitIdeaDialog>(string.Empty));
            cut.Find("#idea-cancel").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task SubmitButtonMustBeDisabledWhenFormIsNotValid()
        {
            // Arrange
            var cut = RenderComponent<MudDialogProvider>();
            var dialogService = Services.GetService<IDialogService>() as DialogService;

            // Act
            _ = await cut.InvokeAsync(() => dialogService?.Show<SubmitIdeaDialog>(string.Empty));
            var submitButton = cut.FindComponents<MudButton>().First(component => component.Instance.FieldId.Equals("idea-submit", StringComparison.Ordinal));

            // Assert
            _ = submitButton.Instance.Disabled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task SubmitButtonMustNotBeDisabledWhenFormIsValid()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();
            var cut = RenderComponent<MudDialogProvider>();
            var dialogService = Services.GetService<IDialogService>() as DialogService;

            _ = await cut.InvokeAsync(() => dialogService?.Show<SubmitIdeaDialog>(string.Empty));

            var titleField = cut.FindComponents<MudTextField<string>>()
                .First(component => component.Instance.FieldId.Equals("idea-title", StringComparison.Ordinal));

            var descriptionField = cut.FindComponents<MudTextField<string>>()
                .First(component => component.Instance.FieldId.Equals("idea-description", StringComparison.Ordinal));

            await cut.InvokeAsync(() => titleField.Instance.SetText(ideaRequest.Title));
            await cut.InvokeAsync(() => descriptionField.Instance.SetText(ideaRequest.Body));

            // Act
            await cut.InvokeAsync(() => cut.FindComponent<MudForm>().Instance.Validate());
            var submitButton = cut.FindComponents<MudButton>().First(component => component.Instance.FieldId.Equals("idea-submit", StringComparison.Ordinal));

            // Assert
            _ = submitButton.Instance.Disabled.Should().BeFalse();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task IdeaShouldBeSubmittedOnClickOnSubmit()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();
            var expectedIdeaResponse = Fixture.Create<IdeaResponse>();

            _ = this.mockIdeaClientService.Setup(service => service.SubmitIdea(It.Is<IdeaRequest>(request => ideaRequest.Title.Equals(request.Title, StringComparison.Ordinal) &&
                    ideaRequest.Body.Equals(request.Body, StringComparison.Ordinal))))
                .ReturnsAsync(expectedIdeaResponse);

            var cut = RenderComponent<MudDialogProvider>();
            var dialogService = Services.GetService<IDialogService>() as DialogService;

            _ = await cut.InvokeAsync(() => dialogService?.Show<SubmitIdeaDialog>(string.Empty));

            var titleField = cut.FindComponents<MudTextField<string>>()
                .First(component => component.Instance.FieldId.Equals("idea-title", StringComparison.Ordinal));

            var descriptionField = cut.FindComponents<MudTextField<string>>()
                .First(component => component.Instance.FieldId.Equals("idea-description", StringComparison.Ordinal));

            await cut.InvokeAsync(() => titleField.Instance.SetText(ideaRequest.Title));
            await cut.InvokeAsync(() => descriptionField.Instance.SetText(ideaRequest.Body));

            // Act
            await cut.InvokeAsync(() => cut.FindComponent<MudForm>().Instance.Validate());
            cut.WaitForElement("#idea-submit").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ClickOnSubmitShouldProcessProblemDetailsException()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();

            _ = this.mockIdeaClientService.Setup(service => service.SubmitIdea(It.Is<IdeaRequest>(request =>
                    ideaRequest.Title.Equals(request.Title, StringComparison.Ordinal) &&
                    ideaRequest.Body.Equals(request.Body, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();
            var dialogService = Services.GetService<IDialogService>() as DialogService;

            _ = await cut.InvokeAsync(() => dialogService?.Show<SubmitIdeaDialog>(string.Empty));

            var titleField = cut.FindComponents<MudTextField<string>>()
                .First(component => component.Instance.FieldId.Equals("idea-title", StringComparison.Ordinal));

            var descriptionField = cut.FindComponents<MudTextField<string>>()
                .First(component => component.Instance.FieldId.Equals("idea-description", StringComparison.Ordinal));

            await cut.InvokeAsync(() => titleField.Instance.SetText(ideaRequest.Title));
            await cut.InvokeAsync(() => descriptionField.Instance.SetText(ideaRequest.Body));

            // Act
            await cut.InvokeAsync(() => cut.FindComponent<MudForm>().Instance.Validate());
            cut.WaitForElement("#idea-submit").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task SetConsentToCollectTechnicalDetailsByDefault()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();
            ideaRequest.ConsentToCollectTechnicalDetails = true;
            var expectedIdeaResponse = Fixture.Create<IdeaResponse>();

            _ = this.mockIdeaClientService.Setup(service => service.SubmitIdea(It.Is<IdeaRequest>(request => ideaRequest.Title.Equals(request.Title, StringComparison.Ordinal) &&
                    ideaRequest.Body.Equals(request.Body, StringComparison.Ordinal))))
                .ReturnsAsync(expectedIdeaResponse);

            var cut = RenderComponent<MudDialogProvider>();
            var dialogService = Services.GetService<IDialogService>() as DialogService;

            _ = await cut.InvokeAsync(() => dialogService?.Show<SubmitIdeaDialog>(string.Empty));

            var titleField = cut.FindComponents<MudTextField<string>>()
                .First(component => component.Instance.FieldId.Equals("idea-title", StringComparison.Ordinal));

            var descriptionField = cut.FindComponents<MudTextField<string>>()
                .First(component => component.Instance.FieldId.Equals("idea-description", StringComparison.Ordinal));

            await cut.InvokeAsync(() => titleField.Instance.SetText(ideaRequest.Title));
            await cut.InvokeAsync(() => descriptionField.Instance.SetText(ideaRequest.Body));

            // Act
            await cut.InvokeAsync(() => cut.FindComponent<MudForm>().Instance.Validate());
            cut.WaitForElement("#idea-submit").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}

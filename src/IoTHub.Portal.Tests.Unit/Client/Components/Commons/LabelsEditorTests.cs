// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Commons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using IoTHub.Portal.Client.Components.Commons;
    using Bunit;
    using FluentAssertions;
    using MudBlazor;
    using NUnit.Framework;
    using Shared.Models.v1._0;
    using UnitTests.Bases;

    [TestFixture]
    public class LabelsEditorTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void Render_NonEmptyLabels_LabelsDisplayed()
        {
            // Arrange
            var readOnlyLabels = Fixture.CreateMany<LabelDto>(2).ToList();
            var labels = Fixture.CreateMany<LabelDto>(3).ToList();

            // Act
            var cut = RenderComponent<LabelsEditor>(
                ComponentParameter.CreateParameter(nameof(LabelsEditor.ReadOnlyLabels), readOnlyLabels),
                ComponentParameter.CreateParameter(nameof(LabelsEditor.Labels), labels));

            // Assert
            _ = cut.FindComponents<MudChip>().Count.Should().Be(5);
        }

        [Test]
        public void Render_EmptyReadOnlyLabels_ReadOnlyLabelsOnDisplayDisplayed()
        {
            // Arrange
            var readOnlyLabels = Array.Empty<LabelDto>().ToList();
            var labels = Fixture.CreateMany<LabelDto>(3).ToList();

            // Act
            var cut = RenderComponent<LabelsEditor>(
                ComponentParameter.CreateParameter(nameof(LabelsEditor.ReadOnlyLabels), readOnlyLabels),
                ComponentParameter.CreateParameter(nameof(LabelsEditor.Labels), labels));

            // Assert
            _ = cut.Markup.Should().NotContain("Mandatory labels");
            _ = cut.FindComponents<MudChip>().Count.Should().Be(3);
        }

        [Test]
        public void Render_NullReadOnlyLabels_ReadOnlyLabelsOnDisplayDisplayed()
        {
            // Arrange
            var labels = Fixture.CreateMany<LabelDto>(3).ToList();

            // Act
            var cut = RenderComponent<LabelsEditor>(
                ComponentParameter.CreateParameter(nameof(LabelsEditor.Labels), labels));

            // Assert
            _ = cut.Markup.Should().NotContain("Mandatory labels");
            _ = cut.FindComponents<MudChip>().Count.Should().Be(3);
        }

        [Test]
        public void AddLabel_NewLabel_LabelIsAdded()
        {
            // Arrange
            var expectedLabelName = "test-label";
            var expectedLabelColor = "#180f6fff";
            var labels = Array.Empty<LabelDto>().ToList();

            var cut = RenderComponent<LabelsEditor>(
                ComponentParameter.CreateParameter(nameof(LabelsEditor.Labels), labels));

            cut.FindComponent<MudTextField<string>>().Find("input").Change(expectedLabelName);
            cut.FindComponent<MudColorPicker>().Find("input").Change(expectedLabelColor);

            // Act
            cut.Find("#add-label").Click();

            // Assert
            _ = cut.FindComponents<MudChip>().Count.Should().Be(1);
            _ = labels.Count.Should().Be(1);
        }

        [Test]
        public void AddLabel_ExistingLabel_LabelIsNotAdded()
        {
            // Arrange
            var expectedLabelName = "test-label";
            var expectedLabelColor = "#180f6fff";
            var labels = new List<LabelDto>()
            {
                new LabelDto
                {
                    Name= expectedLabelName,
                    Color = expectedLabelColor
                }
            };

            var cut = RenderComponent<LabelsEditor>(
                ComponentParameter.CreateParameter(nameof(LabelsEditor.Labels), labels));

            cut.FindComponent<MudTextField<string>>().Find("input").Change(expectedLabelName);
            cut.FindComponent<MudColorPicker>().Find("input").Change(expectedLabelColor);

            // Act
            cut.Find("#add-label").Click();

            // Assert
            _ = cut.FindComponents<MudChip>().Count.Should().Be(1);
            _ = labels.Count.Should().Be(1);
        }

        [Test]
        public void AddLabel_ExistingLabelInReadOnlyLabels_LabelIsNotAdded()
        {
            // Arrange
            var expectedLabelName = "test-label";
            var expectedLabelColor = "#180f6fff";
            var readOnlyLabels = new List<LabelDto>()
            {
                new LabelDto
                {
                    Name= expectedLabelName,
                    Color = expectedLabelColor
                }
            };
            var labels = Array.Empty<LabelDto>().ToList();

            var cut = RenderComponent<LabelsEditor>(
                ComponentParameter.CreateParameter(nameof(LabelsEditor.ReadOnlyLabels), readOnlyLabels),
                ComponentParameter.CreateParameter(nameof(LabelsEditor.Labels), labels));

            cut.FindComponent<MudTextField<string>>().Find("input").Change(expectedLabelName);
            cut.FindComponent<MudColorPicker>().Find("input").Change(expectedLabelColor);

            // Act
            cut.Find("#add-label").Click();

            // Assert
            _ = cut.FindComponents<MudChip>().Count.Should().Be(1);
            _ = labels.Count.Should().Be(0);
        }

        [Test]
        public void RemoveLabel_ExistingLabel_LabelIsRemoved()
        {
            // Arrange
            var labels = new List<LabelDto>()
            {
                new LabelDto
                {
                    Name= "test-label",
                    Color = "#180f6fff"
                }
            };

            var cut = RenderComponent<LabelsEditor>(
                ComponentParameter.CreateParameter(nameof(LabelsEditor.Labels), labels));

            // Act
            cut.FindComponent<MudChip>().Find("button").Click();

            // Assert
            _ = cut.FindComponents<MudChip>().Count.Should().Be(0);
            _ = labels.Count.Should().Be(0);
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Commons
{
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
    public class LabelsTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void Render_NonEmptyLabels_LabelsDisplayed()
        {
            // Arrange
            var labels = Fixture.CreateMany<LabelDto>(3)
                                    .ToList();

            // Act
            var cut = RenderComponent<Labels>(
                ComponentParameter.CreateParameter(nameof(Labels.Items), labels));

            // Assert
            _ = cut.FindComponents<MudChip>().Count.Should().Be(3);
        }

        [Test]
        public void Render_Should_Render_Max_ItemsCount()
        {
            // Arrange
            var labels = Fixture.CreateMany<LabelDto>(10)
                                    .ToList();

            // Act
            var cut = RenderComponent<Labels>(
                ComponentParameter.CreateParameter(nameof(Labels.Items), labels),
                ComponentParameter.CreateParameter(nameof(Labels.MaxDisplayedLabels), 7));

            // Assert
            _ = cut.FindComponents<MudChip>().Count.Should().Be(7);
        }

        [Test]
        public void Render_SearchedLabels_ShouldBeRenderedFirst()
        {
            // Arrange
            var searchedLabels = Fixture.CreateMany<LabelDto>(3).ToList();
            var labels = searchedLabels.Union(Fixture.CreateMany<LabelDto>(3).ToList());

            // Act
            var cut = RenderComponent<Labels>(
                ComponentParameter.CreateParameter(nameof(Labels.SearchedLabels), searchedLabels),
                ComponentParameter.CreateParameter(nameof(Labels.Items), labels),
                ComponentParameter.CreateParameter(nameof(Labels.MaxDisplayedLabels), 4));

            // Assert
            var chips = cut.FindComponents<MudChip>();
            _ = chips.Count.Should().Be(4);
            _ = searchedLabels.Should().AllSatisfy(c => chips.Any(x => x.Instance.Text == c.Name));
        }

        [Test]
        public void Render_SearchedLabelsNotInLabels_ShouldNotBeRendered()
        {
            // Arrange
            var searchedLabels = Fixture.CreateMany<LabelDto>(3)
                                        .ToList();
            var labels = searchedLabels.Take(2)
                            .Union(Fixture.CreateMany<LabelDto>(1).ToList());

            // Act
            var cut = RenderComponent<Labels>(
                ComponentParameter.CreateParameter(nameof(Labels.SearchedLabels), searchedLabels),
                ComponentParameter.CreateParameter(nameof(Labels.Items), labels),
                ComponentParameter.CreateParameter(nameof(Labels.MaxDisplayedLabels), 10));

            // Assert
            var chips = cut.FindComponents<MudChip>();
            _ = chips.Count.Should().Be(3);
            _ = labels.Should().AllSatisfy(c => chips.Any(x => x.Instance.Text == c.Name));
            _ = chips.Should().NotContain(c => c.Instance.Text == searchedLabels.Last().Name);
        }
    }
}

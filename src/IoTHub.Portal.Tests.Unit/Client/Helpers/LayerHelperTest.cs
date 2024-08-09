// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Helpers
{
    using AutoFixture;
    using IoTHub.Portal.Shared.Models.v10;
    using NUnit.Framework;
    using FluentAssertions;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using IoTHub.Portal.Client.Helpers;
    using System.Collections.Generic;
    using IoTHub.Portal.Client.Models;
    using System.Linq;

    internal class LayerHelperTest : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void GetHashsetLayerShouldReturnLayerHash()
        {
            LayerDto expectedLayerDto = Fixture.Create<LayerDto>();
            expectedLayerDto.Name = "Main Layer";
            expectedLayerDto.Father = null;

            var expectedChildrenLayerDto1 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto1.Father = expectedLayerDto.Id;

            var expectedChildrenLayerDto2 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto2.Father = expectedLayerDto.Id;

            var expectedChildrenLayerDto21 = Fixture.Create<LayerDto>();
            expectedChildrenLayerDto21.Father = expectedChildrenLayerDto2.Id;

            HashSet<LayerHash> expectedHash = new HashSet<LayerHash>
            {
                new LayerHash(expectedLayerDto)
            };

            expectedHash.First().Children.Add(new LayerHash(expectedChildrenLayerDto1, 1, false));
            expectedHash.First().Children.Add(new LayerHash(expectedChildrenLayerDto2, 1, false));
            expectedHash.First().Children.Last().Children.Add(new LayerHash(expectedChildrenLayerDto21, 2, false));

            // Act
            var result = LayerHelper.GetHashsetLayer(new List<LayerDto> {expectedLayerDto, expectedChildrenLayerDto1, expectedChildrenLayerDto2, expectedChildrenLayerDto21 });

            // Assert
            _ = result.Should().BeEquivalentTo(expectedHash);

            this.MockRepository.VerifyAll();
        }
    }
}

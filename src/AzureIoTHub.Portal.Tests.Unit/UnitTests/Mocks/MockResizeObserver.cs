// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.UnitTests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using MudBlazor.Interop;
    using MudBlazor.Services;

    /// <summary>
    /// Mock of ResizeObserver, required to unit tests on components containing MudTabs
    /// </summary>
    public class MockResizeObserver : IResizeObserver
    {
        private readonly Dictionary<ElementReference, BoundingClientRect> cachedValues = new();

        public bool IsVertical { get; set; }

        public event SizeChanged OnResized;

        public void UpdateTotalPanelSize(double newSize)
        {
            var entry = this.cachedValues.Last();

            if (!IsVertical)
            {
                entry.Value.Width = newSize;
            }
            else
            {
                entry.Value.Height = newSize;
            }

            OnResized?.Invoke(new Dictionary<ElementReference, BoundingClientRect> {
                    { entry.Key, entry.Value  },
                });
        }

        public void UpdatePanelSize(int index, double newSize)
        {
            var entry = this.cachedValues.ElementAt(index);

            if (!IsVertical)
            {
                entry.Value.Width = newSize;
            }
            else
            {
                entry.Value.Height = newSize;
            }

            OnResized?.Invoke(new Dictionary<ElementReference, BoundingClientRect> {
                    { entry.Key, entry.Value  },
                });
        }

        public double PanelSize { get; set; } = 250;
        public double PanelTotalSize { get; set; } = 3000;

        public async Task<BoundingClientRect> Observe(ElementReference element)
        {
            return (await Observe(new[] { element })).FirstOrDefault();
        }

        private bool firstBatchProcessed;

        public Task<IEnumerable<BoundingClientRect>> Observe(IEnumerable<ElementReference> elements)
        {
            var elementReferences = elements.ToList();
            foreach (var item in elementReferences)
            {
                var size = PanelSize;
                // last element is always TabsContentSize
                if (item.Id == elementReferences.Last().Id && !this.firstBatchProcessed)
                {
                    size = PanelTotalSize;
                }
                var rect = new BoundingClientRect { Width = size };
                if (IsVertical)
                {
                    rect = new BoundingClientRect { Height = size };
                }
                this.cachedValues.Add(item, rect);
            }

            this.firstBatchProcessed = true;

            return Task.FromResult<IEnumerable<BoundingClientRect>>(new List<BoundingClientRect>());
        }

        public Task Unobserve(ElementReference element)
        {
            _ = this.cachedValues.Remove(element);
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public BoundingClientRect GetSizeInfo(ElementReference reference)
        {
            return !this.cachedValues.ContainsKey(reference) ? null : this.cachedValues[reference];
        }
        public double GetHeight(ElementReference reference) => GetSizeInfo(reference)?.Height ?? 0.0;
        public double GetWidth(ElementReference reference) => GetSizeInfo(reference)?.Width ?? 0.0;
        public bool IsElementObserved(ElementReference reference) => this.cachedValues.ContainsKey(reference);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}

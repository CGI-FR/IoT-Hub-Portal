// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Validators
{
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Portal.Shared.Models.v1._0;

    public class DevicePropertyValidator : AbstractValidator<IEnumerable<DeviceProperty>>
    {
        private class DevicePropertyComparer : IEqualityComparer<DeviceProperty>
        {
            public bool Equals(DeviceProperty? x, DeviceProperty? y) => x?.Name == y?.Name;
            public int GetHashCode(DeviceProperty obj) => obj.Name?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
        }

        public DevicePropertyValidator()
        {
            _ = RuleForEach(x => x)
                .NotNull()
                .WithMessage("Property cannot be null.");

            _ = RuleFor(x => x)
                .Must(x => x.Distinct(new DevicePropertyComparer()).Count() == x.Count())
                .WithMessage("Properties should have unique name.");

            _ = RuleForEach(x => x)
                .ChildRules(c =>
                {
                    _ = c.RuleFor(c => c.DisplayName)
                        .NotNull().WithMessage("Property DisplayName is required.")
                        .NotEmpty().WithMessage("Property DisplayName is required.");
                    _ = c.RuleFor(c => c.Name)
                        .NotNull().WithMessage("Property Name is required.")
                        .NotEmpty().WithMessage("Property Name is required.");
                    _ = c.RuleFor(c => c.PropertyType)
                        .NotNull()
                        .WithMessage("Property PropertyType is required.");
                    _ = c.RuleFor(c => c.IsWritable)
                        .NotNull()
                        .WithMessage("Property IsWritable is required.");
                });
        }
    }
}

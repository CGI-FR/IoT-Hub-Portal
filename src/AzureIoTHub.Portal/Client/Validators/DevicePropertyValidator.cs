// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Shared.Models.v10;
    using FluentValidation;
    using System.Collections.Generic;
    using System.Linq;

    public class DevicePropertyValidator : AbstractValidator<IEnumerable<DeviceProperty>>
    {
        private class DevicePropertyComparer : IEqualityComparer<DeviceProperty>
        {
            public bool Equals(DeviceProperty x, DeviceProperty y) => x?.Name == y?.Name;
            public int GetHashCode(DeviceProperty obj) => obj.Name?.GetHashCode() ?? 0;
        }

        public DevicePropertyValidator()
        {
            _ = RuleForEach(x => x)
                .NotNull();

            _ = RuleFor(x => x)
                .Must(x => x.Distinct(new DevicePropertyComparer()).Count() == x.Count())
                .WithMessage("Properties should have unique name.");

            _ = RuleForEach(x => x)
                .ChildRules(c =>
                {
                    _ = c.RuleFor(c => c.DisplayName).NotNull();
                    _ = c.RuleFor(c => c.Name).NotNull();
                    _ = c.RuleFor(c => c.PropertyType).NotNull();
                    _ = c.RuleFor(c => c.IsWritable).NotNull();
                });
        }
    }
}

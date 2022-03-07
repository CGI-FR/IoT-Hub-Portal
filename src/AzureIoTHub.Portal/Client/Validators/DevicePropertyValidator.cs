// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Shared.Models.V10;
    using FluentValidation;
    using System.Collections.Generic;
    using System.Linq;

    public class DevicePropertyValidator : AbstractValidator<IEnumerable<DeviceProperty>>
    {
        private class DevicePropertyComparer : IEqualityComparer<DeviceProperty>
        {
            public bool Equals(DeviceProperty x, DeviceProperty y) => x?.Name == y?.Name;
            public int GetHashCode(DeviceProperty obj) => obj.Name.GetHashCode();
        }

        public DevicePropertyValidator()
        {
            RuleForEach(x => x)
                .NotNull();

            RuleFor(x => x)
                .Must(x => x.Distinct(new DevicePropertyComparer()).Count() == x.Count())
                .WithMessage("Properties should have unique name.");

            RuleForEach(x => x)
                .ChildRules(c =>
                {
                    c.RuleFor(c => c.DisplayName).NotNull();
                    c.RuleFor(c => c.Name).NotNull();
                    c.RuleFor(c => c.PropertyType).NotNull();
                    c.RuleFor(c => c.IsWritable).NotNull();
                });
        }
    }
}

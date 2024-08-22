// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Validators
{
    using System;
    using System.Collections.Generic;
    using FluentValidation;
    using Portal.Shared.Models.v1._0;

    public class IoTEdgeModuleValidator : AbstractValidator<IEnumerable<IoTEdgeModule>>
    {
        private class IoTEdgeModuleComparer : IEqualityComparer<IoTEdgeModule>
        {
            public bool Equals(IoTEdgeModule? x, IoTEdgeModule? y)
            {
                return x?.ModuleName == y?.ModuleName;
            }

            public int GetHashCode(IoTEdgeModule obj)
            {
                return obj.ModuleName?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
            }
        }
        public IoTEdgeModuleValidator()
        {
            _ = RuleForEach(x => x)
                .NotNull()
                .WithMessage("Modules cannot be null.");

            _ = RuleForEach(x => x)
                .ChildRules(c =>
                {
                    _ = c.RuleFor(x => x.ModuleName)
                    .NotEmpty()
                    .WithMessage("Module name is required.");

                    _ = c.RuleFor(x => x.ImageURI)
                    .NotEmpty()
                    .WithMessage("Image uri is required.");
                });
        }
    }
}

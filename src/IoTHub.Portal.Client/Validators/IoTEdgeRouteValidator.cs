// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Validators
{
    using System.Collections.Generic;
    using FluentValidation;
    using Portal.Shared.Models.v1._0;

    public class IoTEdgeRouteValidator : AbstractValidator<IEnumerable<IoTEdgeRoute>>
    {
        public IoTEdgeRouteValidator()
        {
            _ = RuleForEach(x => x)
                .NotNull()
                .WithMessage("Route cannot be null.");

            _ = RuleForEach(x => x)
                .ChildRules(c =>
                {
                    _ = c.RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithMessage("The route name is required.");

                    _ = c.RuleFor(x => x.Value)
                        .NotEmpty()
                        .WithMessage("The route value is required.")
                        .Matches(@"^(?i)FROM [\S]+( WHERE (NOT )?[\S]+)? INTO [\S]+$")
                        .WithMessage("Route should be 'FROM <source> (WHERE <condition>) INTO <sink>'.");

                    _ = c.RuleFor(x => x.Priority)
                        .InclusiveBetween(0, 9)
                        .WithMessage("The priority should be between 0 and 9.");

                    _ = c.RuleFor(x => x.TimeToLive)
                        .InclusiveBetween((uint)0, uint.MaxValue)
                        .WithMessage($"The TimeToLive should be between 0 and {uint.MaxValue} secs.");
                });
        }
    }
}

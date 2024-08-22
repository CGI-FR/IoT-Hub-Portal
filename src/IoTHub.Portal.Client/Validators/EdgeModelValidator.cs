// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Validators
{
    using FluentValidation;
    using Portal.Shared.Models.v1._0;

    public class EdgeModelValidator : AbstractValidator<IoTEdgeModel>
    {
        public EdgeModelValidator()
        {
            _ = RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Model name is required.");
        }
    }
}

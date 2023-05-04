// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Models.v10.AWS;
    using FluentValidation;

    public class ThingTypeValidator : AbstractValidator<ThingTypeDto>
    {
        public ThingTypeValidator()
        {
            _ = RuleFor(x => x.ThingTypeName)
                .NotEmpty()
                .WithMessage("Model name is required.");
        }
    }
}

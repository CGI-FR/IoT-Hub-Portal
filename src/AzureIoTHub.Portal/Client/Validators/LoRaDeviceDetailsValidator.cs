// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using FluentValidation;

    public class LoRaDeviceDetailsValidator : AbstractValidator<LoRaDeviceDetails>
    {
        public LoRaDeviceDetailsValidator()
        {
            _ = RuleFor(x => x.AppEUI)
                .NotEmpty()
                .When(x => x.UseOTAA);

            _ = RuleFor(x => x.AppKey)
                .NotEmpty()
                .When(x => x.UseOTAA);

            _ = RuleFor(x => x.ModelId)
                .NotEmpty();
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<LoRaDeviceDetails>.CreateWithOptions((LoRaDeviceDetails)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}

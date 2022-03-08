// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LoRaDeviceDetailsValidator : AbstractValidator<LoRaDeviceDetails>
    {
        public LoRaDeviceDetailsValidator()
        {
            RuleFor(x => x.AppEUI)
                .NotEmpty()
                .When(x => x.IsOTAAsetting);

            RuleFor(x => x.AppKey)
                .NotEmpty()
                .When(x => x.IsOTAAsetting);

            RuleFor(x => x.ModelId)
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

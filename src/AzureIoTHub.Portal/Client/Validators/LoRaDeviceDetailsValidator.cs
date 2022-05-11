// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using FluentValidation;

    public class LoRaDeviceDetailsValidator : AbstractValidator<LoRaDeviceDetails>
    {
        public LoRaDeviceDetailsValidator()
        {
            _ = RuleFor(x => x.ModelId)
                .NotEmpty()
                .WithMessage("ModelId is required.");

            // OTAA Settings

            _ = RuleFor(x => x.AppEUI)
                .NotEmpty()
                .When(x => x.UseOTAA)
                .WithMessage("AppEUI is required.");

            _ = RuleFor(x => x.AppKey)
                .NotEmpty()
                .When(x => x.UseOTAA)
                .WithMessage("AppKey is required.");

            // ABP Settings

            _ = RuleFor(x => x.AppSKey)
                .NotEmpty()
                .When(x => !x.UseOTAA)
                .WithMessage("AppSKey is required.");

            _ = RuleFor(x => x.NwkSKey)
                .NotEmpty()
                .When(x => !x.UseOTAA)
                .WithMessage("NwkSKey is required.");

            _ = RuleFor(x => x.DevAddr)
                .NotEmpty()
                .When(x => !x.UseOTAA)
                .WithMessage("DevAddr is required.");

            _ = RuleFor(x => x.DeviceID)
                .NotEmpty()
                .Length(1, 128)
                .Matches("[A-Z0-9]{16}")
                .WithMessage("DeviceID is required. It should be a 16 bit hex string.");
        }
    }
}

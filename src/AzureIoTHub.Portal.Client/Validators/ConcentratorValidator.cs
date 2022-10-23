// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using FluentValidation;

    public class ConcentratorValidator : AbstractValidator<ConcentratorDto>
    {
        public ConcentratorValidator()
        {
            _ = RuleFor(x => x.DeviceId)
                .NotEmpty()
                .WithMessage("DeviceId is required.")
                .Matches("(?i)^[A-F0-9]{16}$")
                .WithMessage("DeviceID must contain 16 hexadecimal characters.");

            _ = RuleFor(x => x.DeviceName)
                .NotEmpty()
                .WithMessage("DeviceName is required.");

            _ = RuleFor(x => x.LoraRegion)
                .NotEmpty().WithMessage("LoraRegion is required.")
                .NotNull().WithMessage("LoraRegion is required.");

            _ = RuleFor(x => x.ClientThumbprint)
                .Matches("^(([A-F0-9]{2}:){19}[A-F0-9]{2}|)$")
                .WithMessage("ClientThumbprint must contain 40 hexadecimal characters.");
        }
    }
}

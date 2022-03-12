// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN.LoRaDeviceModel;
    using FluentValidation;

    public class LoRaDeviceModelValidator : AbstractValidator<LoRaDeviceModel>
    {
        public LoRaDeviceModelValidator()
        {
            _ = RuleFor(x => x.AppEUI)
                .NotEmpty()
                .Length(1, 100)
                .When(x => x.UseOTAA);
        }
    }
}

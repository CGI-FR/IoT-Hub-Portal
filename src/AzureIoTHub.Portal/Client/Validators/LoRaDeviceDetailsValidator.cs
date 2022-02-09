// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
    using FluentValidation;

    public class LoRaDeviceDetailsValidator : AbstractValidator<LoRaDeviceDetails>
    {
        public LoRaDeviceDetailsValidator()
        {
            RuleFor(x => x.AppEUI)
                .NotEmpty();

            RuleFor(x => x.AppKey)
                .NotEmpty();

            RuleFor(x => x.ModelId)
                .NotEmpty();
        }
    }
}

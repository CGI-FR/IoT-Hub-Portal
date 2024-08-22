// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using Portal.Shared.Models.v1._0.LoRaWAN;

    public class LoRaDeviceModelCommandValidator : AbstractValidator<DeviceModelCommandDto>
    {
        public LoRaDeviceModelCommandValidator()
        {
            _ = RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("The command name is required.");

            _ = RuleFor(x => x.Frame)
                .NotEmpty()
                .WithMessage("The frame is required.")
                .Length(1, 255)
                .WithMessage("The frame should be up to 255 characters long.")
                .Matches("^[0-9a-fA-F]{0,255}$")
                .WithMessage("The frame should only contain hexadecimal characters");

            _ = RuleFor(x => x.Port)
                .NotEmpty()
                .WithMessage("The port number is required")
                .InclusiveBetween(1, 223)
                .WithMessage("The port number should be between 1 and 223.");
        }
    }
}

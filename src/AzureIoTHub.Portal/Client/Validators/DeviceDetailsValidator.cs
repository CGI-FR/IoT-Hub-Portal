// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using FluentValidation;

    public class DeviceDetailsValidator : AbstractValidator<DeviceDetails>
    {
        public DeviceDetailsValidator()
        {
            _ = RuleFor(x => x.DeviceName)
                .NotEmpty()
                .WithMessage("DeviceName is required.");

            _ = RuleFor(x => x.ModelId)
                .NotEmpty()
                .WithMessage("ModelId is required.");

            _ = RuleFor(x => x.DeviceID)
                .NotEmpty()
                .WithMessage("DeviceID is required.");
        }
    }
}

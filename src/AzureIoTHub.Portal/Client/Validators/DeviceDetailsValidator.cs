// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
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
                .WithMessage("DeviceID is required.")
                .Length(1, 128)
                .Matches("[a-zA-Z0-9\\-.+%_#*?!(),:=@$']")
                .WithMessage("DeviceID is required. It should be a case-sensitive string (up to 128 characters long) of ASCII 7-bit alphanumeric characters plus certain special characters: - . + % _ # * ? ! ( ) , : = @ $ '.");
        }
    }
}

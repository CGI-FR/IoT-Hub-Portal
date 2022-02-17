// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using FluentValidation;

    public class DeviceDetailsValidator : AbstractValidator<DeviceDetails>
    {
        public DeviceDetailsValidator()
        {
            RuleFor(x => x.DeviceName)
                .NotEmpty();

            RuleFor(x => x.ModelId)
                .NotEmpty();
        }
    }
}

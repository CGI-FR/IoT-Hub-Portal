// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Constants;
    using FluentValidation;
    using Portal.Shared.Models.v1._0;

    public class DeviceDetailsValidator : AbstractValidator<DeviceDetails>
    {
        public DeviceDetailsValidator(string? cloudProvider = null)
        {
            _ = RuleFor(x => x.DeviceName)
                .NotEmpty()
                .WithMessage("DeviceName is required.");

            _ = RuleFor(x => x.ModelId)
                .NotEmpty()
                .WithMessage("ModelId is required.");

            _ = When(x => x.IsLoraWan, () =>
            {
                _ = RuleFor(x => x.DeviceID)
                .NotEmpty()
                .Length(1, 128)
                .Matches("[A-Z0-9]{16}")
                .WithMessage("DeviceID is required. It should be a 16 bit hex string.");
            });

            if (cloudProvider != null && cloudProvider.Equals(CloudProviders.Azure, StringComparison.Ordinal))
            {
                _ = RuleFor(x => x.DeviceID)
                .NotEmpty()
                .WithMessage("DeviceID is required.")
                .Length(1, 128)
                .Matches("[a-zA-Z0-9\\-.+%_#*?!(),:=@$']")
                .WithMessage("DeviceID is required. It should be a case-sensitive string (up to 128 characters long) of ASCII 7-bit alphanumeric characters plus certain special characters: - . + % _ # * ? ! ( ) , : = @ $ '.");
            }
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<DeviceDetails>.CreateWithOptions((DeviceDetails)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}

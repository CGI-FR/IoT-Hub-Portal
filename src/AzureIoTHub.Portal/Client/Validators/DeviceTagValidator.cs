// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Validators
{
    using AzureIoTHub.Portal.Shared.Models.v10.Device;
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DeviceTagValidator : AbstractValidator<DeviceTag>
    {
        public DeviceTagValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Label)
                .NotEmpty();

        }

        //public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        //{
        //    var result = await ValidateAsync(ValidationContext<DeviceTag>.CreateWithOptions((DeviceTag)model, x => x.IncludeProperties(propertyName)));
        //    if (result.IsValid)
        //        return Array.Empty<string>();
        //    return result.Errors.Select(e => e.ErrorMessage);
        //};
    }
}

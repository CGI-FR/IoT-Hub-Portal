// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Extensions
{
    using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

    public static class ProblemDetailsOptionsExtensions
    {
        public static void MapFluentValidationException(this ProblemDetailsOptions options) =>
            options.Map<ValidationException>((ctx, ex) =>
            {
                var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                var errors = ex.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(x => x.ErrorMessage).ToArray());

                return factory.CreateValidationProblemDetails(ctx, errors);
            });
    }
}

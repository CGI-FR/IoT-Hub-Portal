// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Extensions
{
    public static class ProblemDetailsExtensions
    {
        public static string ToJson(this ProblemDetailsWithExceptionDetails problemDetails)
        {
            return JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

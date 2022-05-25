// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Extensions
{
    using Models;
    using Newtonsoft.Json;

    public static class ProblemDetailsExtensions
    {
        public static string ToJson(this ProblemDetailsWithExceptionDetails problemDetails)
        {
            return JsonConvert.SerializeObject(problemDetails, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }
    }
}

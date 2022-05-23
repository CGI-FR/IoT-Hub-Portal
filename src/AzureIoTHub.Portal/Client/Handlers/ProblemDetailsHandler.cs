// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Handlers
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;

    public class ProblemDetailsHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsWithExceptionDetails>(cancellationToken: cancellationToken);
            throw new ProblemDetailsException(problemDetails);
        }
    }
}

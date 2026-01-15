// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Handlers
{
    using Severity = MudBlazor.Severity;

    public class ProblemDetailsHandler : DelegatingHandler
    {
        private readonly ISnackbar snackbar;
        private readonly NavigationManager navigationManager;

        public ProblemDetailsHandler(ISnackbar snackbar, NavigationManager navigationManager)
        {
            this.snackbar = snackbar;
            this.navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            if (response.StatusCode.Equals(HttpStatusCode.Unauthorized))
            {
                _ = this.snackbar.Add("You are not authorized", Severity.Error, config =>
                {
                    config.Action = "Got to login";
                    config.ActionColor = Color.Primary;
                    config.RequireInteraction = true;
                    config.Onclick = _ =>
                    {
                        var returnUrl = this.navigationManager.ToBaseRelativePath(this.navigationManager.Uri);
                        this.navigationManager.NavigateToLogin($"authentication/login?returnUrl={returnUrl}");

                        return Task.CompletedTask;
                    };
                });

            }

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsWithExceptionDetails>(cancellationToken: cancellationToken);

            throw new ProblemDetailsException(problemDetails);
        }
    }
}

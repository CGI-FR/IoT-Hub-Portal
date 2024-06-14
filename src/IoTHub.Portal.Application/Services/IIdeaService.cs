// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using Shared.Models.v10;

    public interface IIdeaService
    {
        Task<IdeaResponse> SubmitIdea(IdeaRequest ideaRequest, string? userAgent = null);
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v1._0
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models.v1._0;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/ideas")]
    [ApiExplorerSettings(GroupName = "Ideas")]
    public class IdeasController : ControllerBase
    {
        private readonly IIdeaService ideasService;

        public IdeasController(IIdeaService ideasService)
        {
            this.ideasService = ideasService;
        }

        [HttpPost(Name = "Submit Idea to Iot Hub Portal community")]
        public Task<IdeaResponse> SubmitIdea([FromBody] IdeaRequest ideaRequest)
        {
            return this.ideasService.SubmitIdea(ideaRequest);
        }
    }
}

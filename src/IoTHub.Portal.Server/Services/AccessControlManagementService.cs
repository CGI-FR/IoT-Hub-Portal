// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information


namespace IoTHub.Portal.Server.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AccessControlManagementService : IAccessControlManagementService
    {
        private readonly IAccessControlRepository accessControlRepository;
        private readonly IMapper mapper;

        public AccessControlManagementService(IAccessControlRepository accessControlRepository, IMapper mapper)
        {
            this.accessControlRepository = accessControlRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AccessControlDto>> GetAllAccessControlsAsync()
        {
            var accessControls = await accessControlRepository.GetAllAsync();

            var accessControlDtos = accessControls.Select(accessControl => mapper.Map<AccessControlDto>(accessControl));

            return accessControlDtos;
        }
        public async Task<AccessControlDto> GetAccessControlByIdAsync(string accessControlId)
        {
            var accessControl = await accessControlRepository.GetByIdAsync(accessControlId);
            return accessControl != null ? mapper.Map<AccessControlDto>(accessControl) : null;
        }

        public Task GetAllAccessControlAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}

// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Crosscutting;

    internal class AccessControlService : IAccessControlManagementService
    {
        private readonly IAccessControlRepository accessControlRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IRoleRepository roleRepository;

        public AccessControlService(IAccessControlRepository accessControlRepository, IUnitOfWork unitOfWork, IMapper mapper, IRoleRepository roleRepository)
        {
            this.accessControlRepository = accessControlRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.roleRepository = roleRepository;
        }

        public async Task<AccessControlModel> GetAccessControlAsync(string Id)
        {
            var acEntity = await this.accessControlRepository.GetByIdAsync(Id, ac => ac.Role);
            if (acEntity is null)
            {
                throw new ResourceNotFoundException($"The AccessControl with the id {Id} doesn't exist");
            }
            var acModel = this.mapper.Map<AccessControlModel>(acEntity);
            return acModel;
        }

        public async Task<PaginatedResult<AccessControlModel>> GetAccessControlPage(
            string? searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null
            )
        {
            var acFilter = new AccessControlFilter
            {
                Keyword = searchKeyword,
                PageSize = pageSize,
                PageNumber = pageNumber,
                OrderBy = orderBy
            };
            var acPredicate = PredicateBuilder.True<AccessControl>();
            if (!string.IsNullOrWhiteSpace(acFilter.Keyword))
            {
                acPredicate = acPredicate.And(ac => ac.Scope.ToLower().Contains(acFilter.Keyword.ToLower()) || ac.Role.Name.ToLower().Contains(acFilter.Keyword.ToLower()));
            }

            var paginatedAc = await this.accessControlRepository.GetPaginatedListWithDetailsAsync(
                pageNumber,
                pageSize,
                orderBy,
                acPredicate
            );

            var paginatedAcDto = new PaginatedResult<AccessControlModel>
            {
                Data = paginatedAc.Data.Select(x => this.mapper.Map<AccessControlModel>(x)).ToList(),
                TotalCount = paginatedAc.TotalCount,
                CurrentPage = paginatedAc.CurrentPage,
                PageSize = pageSize
            };
            return new PaginatedResult<AccessControlModel>(paginatedAcDto.Data, paginatedAcDto.TotalCount);
        }

        Task<AccessControlModel> IAccessControlManagementService.CreateAccessControl(AccessControlModel role)
        {
            throw new NotImplementedException();
        }

        async Task<bool> IAccessControlManagementService.DeleteAccessControl(string id)
        {
            var accessControl = await accessControlRepository.GetByIdAsync(id);
            if (accessControl is null)
            {
                throw new ResourceNotFoundException("$The AccessControl with the id { Id } that you want to delete doesn't exist");
            }
            accessControlRepository.Delete(id);
            return true;
        }

        Task<AccessControlModel?> IAccessControlManagementService.UpdateAccessControl(string id, AccessControlModel accessControl)
        {
            throw new NotImplementedException();
        }
    }
}

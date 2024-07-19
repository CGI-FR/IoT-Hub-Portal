// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
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
        private readonly IPrincipalRepository principalRepository;

        public AccessControlService(IAccessControlRepository accessControlRepository, IUnitOfWork unitOfWork, IMapper mapper, IRoleRepository roleRepository, IPrincipalRepository principalRepository)
        {
            this.accessControlRepository = accessControlRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.roleRepository = roleRepository;
            this.principalRepository = principalRepository;
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
            string[] orderBy = null,
            string? principalId = null
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

            if (!string.IsNullOrEmpty(principalId))
            {
                acPredicate = acPredicate.And(ac => ac.PrincipalId == principalId);
            }

            var paginatedAc = await this.accessControlRepository.GetPaginatedListAsync(
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

        public async Task<AccessControlModel> CreateAccessControl(AccessControlModel accessControl)
        {
            if (accessControl is null)
            {
                throw new ArgumentNullException(nameof(accessControl));
            }
            var principal = await this.principalRepository.GetByIdAsync(accessControl.PrincipalId);
            if (principal == null)
            {
                throw new ResourceNotFoundException($"The principal with the id {accessControl.PrincipalId} does'nt exist !");
            }
            var role = await this.roleRepository.GetByIdAsync(accessControl.Role.Id);
            if (role == null)
            {
                throw new ResourceNotFoundException($"The role {accessControl.Role.Name} with the id {accessControl.Role.Id} does'nt exist !");
            }
            var acEntity = this.mapper.Map<AccessControl>(accessControl);
            await this.accessControlRepository.InsertAsync(acEntity);
            await this.unitOfWork.SaveAsync();

            var createdAc = await this.accessControlRepository.GetByIdAsync(acEntity.Id, ac => ac.Role);
            var createdModel = this.mapper.Map<AccessControlModel>(createdAc);
            return createdModel;

        }
        public async Task<AccessControlModel?> UpdateAccessControl(string id, AccessControlModel accessControl)
        {
            if (accessControl is null)
            {
                throw new ArgumentNullException(nameof(accessControl));
            }
            var acEntity = await this.accessControlRepository.GetByIdAsync(id, ac => ac.Role);
            if (acEntity is null)
            {
                throw new ResourceNotFoundException($"The AccessControl with the id {id} doesn't exist");
            }
            var principal = await this.principalRepository.GetByIdAsync(accessControl.PrincipalId);
            if (principal is null)
            {
                throw new ResourceNotFoundException($"The principal with the id {accessControl.PrincipalId} not found !");
            }
            var role = await this.roleRepository.GetByIdAsync(accessControl.Role.Id);
            if (role is null)
            {
                throw new ResourceNotFoundException($"Specified role with the id {accessControl.Role.Id} not found");
            }
            acEntity.PrincipalId = accessControl.PrincipalId;
            acEntity.RoleId = accessControl.Role.Id;
            acEntity.Scope = accessControl.Scope;
            accessControlRepository.Update(acEntity);
            await this.unitOfWork.SaveAsync();

            var createdAc = await this.accessControlRepository.GetByIdAsync(id, ac => ac.Role);
            return this.mapper.Map<AccessControlModel>(createdAc);
        }

        public async Task<bool> DeleteAccessControl(string id)
        {
            var acEntity = await this.accessControlRepository.GetByIdAsync(id);
            if (acEntity is null)
            {
                throw new ResourceNotFoundException($"The AccessControl with the id {id} doesn't exist");
            }
            accessControlRepository.Delete(id);
            await this.unitOfWork.SaveAsync();
            return true;
        }
    }
}

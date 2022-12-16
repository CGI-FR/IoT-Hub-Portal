// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.AspNetCore.Http;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;

    public class EdgeModelService : IEdgeModelService
    {

        /// <summary>
        /// The Configurations service.
        /// </summary>
        private readonly IConfigService configService;

        ///// <summary>
        ///// The device model mapper.
        ///// </summary>
        ////private readonly IEdgeDeviceModelMapper edgeDeviceModelMapper;

        /// <summary>
        /// The device model image manager.
        /// </summary>
        private readonly IDeviceModelImageManager deviceModelImageManager;

        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceModelRepository edgeModelRepository;
        private readonly ILabelRepository labelRepository;
        private readonly IEdgeModuleCommandsService edgeModuleCommandsService;

        public EdgeModelService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceModelRepository edgeModelRepository,
            IConfigService configService,
            IDeviceModelImageManager deviceModelImageManager,
            ILabelRepository labelRepository,
            IEdgeModuleCommandsService edgeModuleCommandsService)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeModelRepository = edgeModelRepository;
            this.labelRepository = labelRepository;
            this.configService = configService;
            this.deviceModelImageManager = deviceModelImageManager;
            this.edgeModuleCommandsService = edgeModuleCommandsService;
        }

        /// <summary>
        /// Return the edge model template list.
        /// </summary>
        /// <returns>IEnumerable IoTEdgeModelListItem.</returns>
        public async Task<IEnumerable<IoTEdgeModelListItem>> GetEdgeModels(EdgeModelFilter edgeModelFilter)
        {
            var edgeDeviceModelPredicate = PredicateBuilder.True<EdgeDeviceModel>();

            if (!string.IsNullOrWhiteSpace(edgeModelFilter.Keyword))
            {
                edgeDeviceModelPredicate = edgeDeviceModelPredicate.And(model => model.Name.ToLower().Contains(edgeModelFilter.Keyword.ToLower()) || model.Description.ToLower().Contains(edgeModelFilter.Keyword.ToLower()));
            }

            var edgeModels = await this.edgeModelRepository
                .GetAllAsync(edgeDeviceModelPredicate, default, m => m.Labels);

            return edgeModels
                .Select(model =>
                {
                    var edgeDeviceModelListItem = this.mapper.Map<IoTEdgeModelListItem>(model);
                    edgeDeviceModelListItem.ImageUrl = this.deviceModelImageManager.ComputeImageUri(edgeDeviceModelListItem.ModelId);
                    return edgeDeviceModelListItem;
                })
                .ToList();
        }

        /// <summary>
        /// Create a new edge model template and roll out
        /// the edge model configuration.
        /// </summary>
        /// <param name="edgeModel">the new edge modle object.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="ResourceAlreadyExistsException">If edge model template already exist return ResourceAlreadyExistsException.</exception>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task CreateEdgeModel(IoTEdgeModel edgeModel)
        {
            var edgeModelEntity = await this.edgeModelRepository.GetByIdAsync(edgeModel?.ModelId);
            if (edgeModelEntity == null)
            {
                edgeModelEntity = this.mapper.Map<EdgeDeviceModel>(edgeModel);
                await this.edgeModelRepository.InsertAsync(edgeModelEntity);
                await this.unitOfWork.SaveAsync();
            }
            else
            {
                throw new ResourceAlreadyExistsException($"The edge model with id {edgeModel?.ModelId} already exists");
            }

            _ = await this.deviceModelImageManager.SetDefaultImageToModel(edgeModel?.ModelId);
            await this.edgeModuleCommandsService.SaveEdgeModuleCommandAsync(edgeModel.ModelId, edgeModel.EdgeModules);
            await this.configService.RollOutEdgeModelConfiguration(edgeModel);
        }

        /// <summary>
        /// Get the edge model template and its configuration by the edge model id.
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        /// <returns>An edge model object.</returns>
        /// <exception cref="ResourceNotFoundException">Resource not found if template does not exist.</exception>
        public async Task<IoTEdgeModel> GetEdgeModel(string modelId)
        {
            var edgeModelEntity = await this.edgeModelRepository.GetByIdAsync(modelId, m => m.Labels);

            if (edgeModelEntity == null)
            {
                throw new ResourceNotFoundException($"The edge model with id {modelId} doesn't exist");
            }

            var modules = await this.configService.GetConfigModuleList(modelId);
            var sysModules = await this.configService.GetModelSystemModule(modelId);
            var routes = await this.configService.GetConfigRouteList(modelId);
            var commands = await  this.edgeModuleCommandsService.GetAllEdgeModuleCommands(modelId);

            var edgeModel = new IoTEdgeModel
            {
                ModelId = edgeModelEntity.Id,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(edgeModelEntity.Id),
                Name = edgeModelEntity.Name,
                Description = edgeModelEntity.Description,
                EdgeModules = modules,
                EdgeRoutes = routes,
                SystemModules = sysModules,
                Labels = this.mapper.Map<List<LabelDto>>(edgeModelEntity.Labels)
            };

            foreach (var command in commands)
            {
                var module = edgeModel.EdgeModules.SingleOrDefault(x => x.ModuleName.Equals(command.EdgeModuleName, StringComparison.Ordinal));
                if (module == null)
                {
                    continue;
                }

                module.Commands.Add(this.mapper.Map<EdgeModuleCommandDto>(command));
            }

            return edgeModel;
        }


        /// <summary>
        /// Update the edge model template and the configuration.
        /// </summary>
        /// <param name="edgeModel">The edge model.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task UpdateEdgeModel(IoTEdgeModel edgeModel)
        {
            var edgeModelEntity = await this.edgeModelRepository.GetByIdAsync(edgeModel?.ModelId, m => m.Labels);
            if (edgeModelEntity == null)
            {
                throw new ResourceNotFoundException($"The edge model with id {edgeModel?.ModelId} doesn't exist");
            }

            foreach (var labelEntity in edgeModelEntity.Labels)
            {
                this.labelRepository.Delete(labelEntity.Id);
            }

            _ = this.mapper.Map(edgeModel, edgeModelEntity);
            this.edgeModelRepository.Update(edgeModelEntity);

            await this.unitOfWork.SaveAsync();

            await this.edgeModuleCommandsService.SaveEdgeModuleCommandAsync(edgeModel.ModelId, edgeModel.EdgeModules);
            await this.configService.RollOutEdgeModelConfiguration(edgeModel);
        }

        /// <summary>
        /// Delete edge model template and its configuration.
        /// </summary>
        /// <param name="edgeModelId">The edge model indentifier.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task DeleteEdgeModel(string edgeModelId)
        {
            var edgeModelEntity = await this.edgeModelRepository.GetByIdAsync(edgeModelId, m => m.Labels);
            if (edgeModelEntity == null)
            {
                return;
            }

            var config = this.configService.GetIoTEdgeConfigurations().Result.FirstOrDefault(x => x.Id.StartsWith(edgeModelId, StringComparison.Ordinal));

            if (config != null)
            {
                await this.configService.DeleteConfiguration(config.Id);
            }

            var existingCommands = await this.edgeModuleCommandsService.GetAllEdgeModuleCommands(edgeModelId);
            foreach (var command in existingCommands)
            {
                this.edgeModuleCommandsService.DeleteEdgeModuleCommandAsync(command.Id);
            }

            foreach (var labelEntity in edgeModelEntity.Labels)
            {
                this.labelRepository.Delete(labelEntity.Id);
            }

            this.edgeModelRepository.Delete(edgeModelId);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Get the edge model avatar.
        /// </summary>
        /// <param name="edgeModelId">The edge model indentifier.</param>
        /// <returns></returns>
        public Task<string> GetEdgeModelAvatar(string edgeModelId)
        {
            return Task.Run(() => this.deviceModelImageManager.ComputeImageUri(edgeModelId).ToString());
        }

        /// <summary>
        /// Update the edge model avatar.
        /// </summary>
        /// <param name="edgeModelId">The edge model indentifier</param>
        /// <param name="file">The image.</param>
        /// <returns></returns>
        public Task<string> UpdateEdgeModelAvatar(string edgeModelId, IFormFile file)
        {
            return Task.Run(() => this.deviceModelImageManager.ChangeDeviceModelImageAsync(edgeModelId, file?.OpenReadStream()));
        }

        /// <summary>
        /// Delete the edge model avatar.
        /// </summary>
        /// <param name="edgeModelId">The edge model indentifier</param>
        /// <returns></returns>
        public Task DeleteEdgeModelAvatar(string edgeModelId)
        {
            return Task.Run(() => this.deviceModelImageManager.DeleteDeviceModelImageAsync(edgeModelId));
        }
    }
}

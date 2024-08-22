// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Shared.Constants;
    using Microsoft.AspNetCore.Http;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.Filters;
    using ResourceNotFoundException = Domain.Exceptions.ResourceNotFoundException;

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

        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceModelRepository edgeModelRepository;
        private readonly ILabelRepository labelRepository;
        private readonly IEdgeDeviceModelCommandRepository commandRepository;
        private readonly ConfigHandler config;
        private readonly IMapper mapper;

        public EdgeModelService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceModelRepository edgeModelRepository,
            IConfigService configService,
            IDeviceModelImageManager deviceModelImageManager,
            ILabelRepository labelRepository,
            IEdgeDeviceModelCommandRepository commandRepository,
            ConfigHandler config)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeModelRepository = edgeModelRepository;
            this.labelRepository = labelRepository;
            this.configService = configService;
            this.deviceModelImageManager = deviceModelImageManager;
            this.commandRepository = commandRepository;
            this.config = config;
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
            }
            else
            {
                throw new ResourceAlreadyExistsException($"The edge model with id {edgeModel?.ModelId} already exists");
            }

            _ = await this.deviceModelImageManager.SetDefaultImageToModel(edgeModel?.ModelId);


            if (this.config.CloudProvider.Equals(CloudProviders.Azure, StringComparison.Ordinal))
            {
                await SaveModuleCommands(edgeModel);
            }

            edgeModelEntity.ExternalIdentifier = await this.configService.RollOutEdgeModelConfiguration(edgeModel);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Saves the module commands for a specific model object.
        /// </summary>
        /// <param name="deviceModelObject">The device model object.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        private async Task SaveModuleCommands(IoTEdgeModel deviceModelObject)
        {
            IEnumerable<IoTEdgeModuleCommand> moduleCommands = deviceModelObject.EdgeModules
                .SelectMany(x => x.Commands.Select(cmd => new IoTEdgeModuleCommand
                {
                    EdgeDeviceModelId = deviceModelObject.ModelId,
                    CommandId = Guid.NewGuid().ToString(),
                    ModuleName = x.ModuleName,
                    Name = cmd.Name,
                })).ToArray();

            var existingCommands = this.commandRepository.GetAll().Where(x => x.EdgeDeviceModelId == deviceModelObject.ModelId).ToList();

            foreach (var command in existingCommands)
            {
                this.commandRepository.Delete(command.Id);
            }

            foreach (var cmd in moduleCommands)
            {
                await this.commandRepository.InsertAsync(this.mapper.Map<EdgeDeviceModelCommand>(cmd));
            }
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
            if (config.CloudProvider.Equals(CloudProviders.Azure, StringComparison.OrdinalIgnoreCase))
            {
                return await GetAzureEdgeModel(edgeModelEntity);
            }
            else
            {
                return await GetAwsEdgeModel(edgeModelEntity);
            }
        }

        private async Task<IoTEdgeModel> GetAzureEdgeModel(EdgeDeviceModel edgeModelEntity)
        {
            var modules = await this.configService.GetConfigModuleList(edgeModelEntity.Id);
            var sysModules = await this.configService.GetModelSystemModule(edgeModelEntity.Id);
            var routes = await this.configService.GetConfigRouteList(edgeModelEntity.Id);
            var commands =  this.commandRepository.GetAll().Where(x => x.EdgeDeviceModelId == edgeModelEntity.Id).ToList();

            //TODO : User a mapper
            //Previously return this.edgeDeviceModelMapper.CreateEdgeDeviceModel(query.Value, modules, routes, commands);
            var result = new IoTEdgeModel
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
                var module = result.EdgeModules.SingleOrDefault(x => x.ModuleName.Equals(command.ModuleName, StringComparison.Ordinal));
                if (module == null)
                {
                    continue;
                }

                module.Commands.Add(this.mapper.Map<IoTEdgeModuleCommand>(command));
            }

            return result;
        }

        private async Task<IoTEdgeModel> GetAwsEdgeModel(EdgeDeviceModel edgeModelEntity)
        {
            var modules = await this.configService.GetConfigModuleList(edgeModelEntity.ExternalIdentifier!);
            //TODO : User a mapper
            //Previously return this.edgeDeviceModelMapper.CreateEdgeDeviceModel(query.Value, modules, routes, commands);
            var result = new IoTEdgeModel
            {
                ModelId = edgeModelEntity.Id,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(edgeModelEntity.Id),
                Name = edgeModelEntity.Name,
                Description = edgeModelEntity.Description,
                EdgeModules = modules,
                Labels = this.mapper.Map<List<LabelDto>>(edgeModelEntity.Labels)
            };

            return result;
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

            edgeModel.ExternalIdentifier = await this.configService.RollOutEdgeModelConfiguration(edgeModel);

            _ = this.mapper.Map(edgeModel, edgeModelEntity);

            this.edgeModelRepository.Update(edgeModelEntity);


            // For AWS, we do the update in the AwsConfiguration
            if (this.config.CloudProvider.Equals(CloudProviders.Azure, StringComparison.Ordinal))
            {
                await SaveModuleCommands(edgeModel);

            }

            await this.unitOfWork.SaveAsync();
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

            if (this.config.CloudProvider.Equals(CloudProviders.Azure, StringComparison.Ordinal))
            {
                var config = this.configService.GetIoTEdgeConfigurations().Result.FirstOrDefault(x => x.Id.StartsWith(edgeModelId, StringComparison.Ordinal));

                if (config != null)
                {
                    await this.configService.DeleteConfiguration(config.Id);
                }

                var existingCommands = this.commandRepository.GetAll().Where(x => x.EdgeDeviceModelId == edgeModelId).ToList();
                foreach (var command in existingCommands)
                {
                    this.commandRepository.Delete(command.Id);
                }
            }
            else
            {
                await this.configService.DeleteConfiguration(edgeModelEntity.ExternalIdentifier!);
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

        public Task<IEnumerable<IoTEdgeModule>> GetPublicEdgeModules()
        {
            return this.configService.GetPublicEdgeModules();
        }
    }
}

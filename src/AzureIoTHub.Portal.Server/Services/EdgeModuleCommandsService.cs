// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;
    using Microsoft.Extensions.Logging;

    public class EdgeModuleCommandsService : IEdgeModuleCommandsService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeModuleCommandMethodManager commandMethodManager;
        private readonly IEdgeModuleCommandsRepository edgeModuleCommandsRepository;
        private readonly IEdgeDeviceModelCommandRepository edgeDeviceModelCommandRepository;
        private readonly ILogger<EdgeModuleCommandsService> logger;

        public EdgeModuleCommandsService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeModuleCommandMethodManager commandMethodManager,
            IEdgeModuleCommandsRepository edgeModuleCommandsRepository,
            IEdgeDeviceModelCommandRepository edgeDeviceModelCommandRepository,
            ILogger<EdgeModuleCommandsService> logger)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.commandMethodManager = commandMethodManager;
            this.edgeModuleCommandsRepository = edgeModuleCommandsRepository;
            this.edgeDeviceModelCommandRepository = edgeDeviceModelCommandRepository;
            this.logger = logger;
        }

        public Task<EdgeModuleCommandDto[]> GetAllEdgeModule(string edgeModelId)
        {
            return Task.Run(() => this.edgeModuleCommandsRepository.GetAll()
            .Where(command => command.EdgeModelId.Equals(edgeModelId, StringComparison.Ordinal))
                .Select(command => this.mapper.Map<EdgeModuleCommandDto>(command))
                .ToArray());
        }

        public async Task SaveEdgeModuleCommandAsync(string edgeModelId, EdgeModuleCommandDto[] commands)
        {
            var edgeModelEntity = await this.edgeDeviceModelCommandRepository.GetByIdAsync(edgeModelId);

            if (edgeModelEntity == null)
            {
                throw new ResourceNotFoundException($"The device model {edgeModelId} doesn't exist");
            }

            var existingCommands = await GetAllEdgeModule(edgeModelId);

            foreach (var command in existingCommands)
            {
                this.edgeModuleCommandsRepository.Delete(command.Id);
            }

            foreach (var edgeDeviceModelCommand in commands)
            {
                var deviceModelCommandEntity = this.mapper.Map<EdgeModuleCommand>(edgeDeviceModelCommand);

                deviceModelCommandEntity.EdgeModelId = edgeModelId;
                await this.edgeModuleCommandsRepository.InsertAsync(deviceModelCommandEntity);
            }

            await this.unitOfWork.SaveAsync();
        }

        public async Task ExecuteModuleCommand(string deviceId, string commandId)
        {
            var commandEntity = await this.edgeModuleCommandsRepository.GetByIdAsync(commandId);

            if (commandEntity == null)
            {
                throw new ResourceNotFoundException($"The command {commandId} for the device {deviceId} cannot be found");
            }

            var result  = await this.commandMethodManager.ExecuteEdgeModuleCommandMessage(deviceId, this.mapper.Map<EdgeModuleCommandDto>(commandEntity));

            if (!result.IsSuccessStatusCode)
            {
                this.logger.LogError($"{deviceId} - Execute command on device failed \n{(int)result.StatusCode} - {result.ReasonPhrase}\n{await result.Content.ReadAsStringAsync()}");

                throw new InternalServerErrorException($"Something went wrong when executing the command {commandEntity.Name}.");
            }

            this.logger.LogInformation($"{deviceId} - Execute command: \n{(int)result.StatusCode} - {result.ReasonPhrase}\n{await result.Content.ReadAsStringAsync()}");
        }
    }
}

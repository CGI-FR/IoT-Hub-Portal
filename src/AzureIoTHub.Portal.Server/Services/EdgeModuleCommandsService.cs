// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;
    using Microsoft.Extensions.Logging;

    public class EdgeModuleCommandsService : IEdgeModuleCommandsService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeModuleCommandMethodManager commandMethodManager;
        private readonly IEdgeModuleCommandsRepository edgeModuleCommandsRepository;
        private readonly ILogger<EdgeModuleCommandsService> logger;

        public EdgeModuleCommandsService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeModuleCommandMethodManager commandMethodManager,
            IEdgeModuleCommandsRepository edgeModuleCommandsRepository,
            ILogger<EdgeModuleCommandsService> logger)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.commandMethodManager = commandMethodManager;
            this.edgeModuleCommandsRepository = edgeModuleCommandsRepository;
            this.logger = logger;
        }

        public Task<IEnumerable<EdgeModuleCommand>> GetAllEdgeModuleCommands(string edgeModelId)
        {
            return Task.Run(() => this.edgeModuleCommandsRepository.GetAll()
            .Where(command => command.EdgeModelId.Equals(edgeModelId, StringComparison.Ordinal)));
        }

        public async Task SaveEdgeModuleCommandAsync(string edgeModelId, List<IoTEdgeModule> edgeModules)
        {
            var existingCommands = await GetAllEdgeModuleCommands(edgeModelId);

            foreach (var command in existingCommands)
            {
                this.edgeModuleCommandsRepository.Delete(command.Id);
            }

            edgeModules.ForEach(x => x.Commands.ForEach(async (cmd) =>
            {
                var deviceModelCommandEntity = this.mapper.Map<EdgeModuleCommand>(cmd);

                deviceModelCommandEntity.EdgeModelId = edgeModelId;
                deviceModelCommandEntity.EdgeModuleName = x.ModuleName;
                await this.edgeModuleCommandsRepository.InsertAsync(deviceModelCommandEntity);
            }));

            await this.unitOfWork.SaveAsync();
        }

        public void DeleteEdgeModuleCommandAsync(string commandId)
        {
            this.edgeModuleCommandsRepository.Delete(commandId);
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

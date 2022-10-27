// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Domain;
    using Domain.Entities;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Managers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Models.v10.LoRaWAN;

    public class LoRaWANCommandService : ILoRaWANCommandService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceModelCommandRepository deviceModelCommandRepository;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly ILoraDeviceMethodManager loraDeviceMethodManager;
        private readonly ILogger<LoRaWANCommandService> logger;

        public LoRaWANCommandService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceModelCommandRepository deviceModelCommandRepository,
            IDeviceModelRepository deviceModelRepository,
            ILoraDeviceMethodManager loraDeviceMethodManager,
            ILogger<LoRaWANCommandService> logger)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceModelCommandRepository = deviceModelCommandRepository;
            this.deviceModelRepository = deviceModelRepository;
            this.loraDeviceMethodManager = loraDeviceMethodManager;
            this.logger = logger;
        }

        public async Task PostDeviceModelCommands(string id, DeviceModelCommandDto[] commands)
        {
            try
            {
                var deviceModelEntity = await this.deviceModelRepository.GetByIdAsync(id);

                if (deviceModelEntity == null)
                {
                    throw new ResourceNotFoundException($"The device model {id} doesn't exist");
                }

                var existingDeviceModelCommands = await GetDeviceModelCommandsFromModel(id);

                foreach (var deviceModelCommand in existingDeviceModelCommands)
                {
                    this.deviceModelCommandRepository.Delete(deviceModelCommand.Name);
                }

                foreach (var deviceModelCommand in commands)
                {
                    var deviceModelCommandEntity = this.mapper.Map<DeviceModelCommand>(deviceModelCommand);
                    deviceModelCommandEntity.DeviceModelId = id;

                    await this.deviceModelCommandRepository.InsertAsync(deviceModelCommandEntity);
                }

                await this.unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to create the commands for the model with id {id}", e);
            }
        }

        public Task<DeviceModelCommandDto[]> GetDeviceModelCommandsFromModel(string id)
        {
            return Task.Run(() => this.deviceModelCommandRepository.GetAll()
                .Where(command => command.DeviceModelId.Equals(id, StringComparison.Ordinal))
                .Select(command => this.mapper.Map<DeviceModelCommandDto>(command))
                .ToArray());
        }

        public async Task ExecuteLoRaWANCommand(string deviceId, string commandId)
        {
            var commandEntity = await this.deviceModelCommandRepository.GetByIdAsync(commandId);

            if (commandEntity == null)
            {
                throw new ResourceNotFoundException($"The LoRaWAN command {commandId} for the device {deviceId} cannot be found");
            }

            var result = await this.loraDeviceMethodManager.ExecuteLoRaDeviceMessage(deviceId, this.mapper.Map<DeviceModelCommandDto>(commandEntity));

            if (!result.IsSuccessStatusCode)
            {
                this.logger.LogError($"{deviceId} - Execute command on device failed \n{(int)result.StatusCode} - {result.ReasonPhrase}\n{await result.Content.ReadAsStringAsync()}");

                throw new InternalServerErrorException($"Something went wrong when executing the command {commandEntity.Name}.");
            }

            this.logger.LogInformation($"{deviceId} - Execute command: \n{(int)result.StatusCode} - {result.ReasonPhrase}\n{await result.Content.ReadAsStringAsync()}");
        }
    }
}

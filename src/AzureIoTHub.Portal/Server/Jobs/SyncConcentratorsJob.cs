// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using Domain.Repositories;
    using Services;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Quartz;

    [DisallowConcurrentExecution]
    public class SyncConcentratorsJob : IJob
    {
        private readonly ILoRaWANConcentratorService concentratorService;
        private readonly IConcentratorRepository concentratorRepository;
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<SyncConcentratorsJob> logger;

        //private const string ModelId = "modelId";

        public SyncConcentratorsJob(ILoRaWANConcentratorService concentratorService,
            IConcentratorRepository concentratorRepository,
            IExternalDeviceService externalDeviceService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<SyncConcentratorsJob> logger)
        {
            this.concentratorService = concentratorService;
            this.concentratorRepository = concentratorRepository;
            this.externalDeviceService = externalDeviceService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync concentrators job");

                await SyncConcentrators();

                this.logger.LogInformation("End of sync concentrators job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync concentrators job has failed");
            }
        }

        private async Task SyncConcentrators()
        {
            foreach (var twin in await GetTwinConcentrators())
            {
                await CreateOrUpdateConcentrator(twin);
            }

            await this.unitOfWork.SaveAsync();
        }

        private async Task<List<Twin>> GetTwinConcentrators()
        {
            var twins = new List<Twin>();
            var continuationToken = string.Empty;

            int totalTwinConcentrators;
            do
            {
                var result = await this.externalDeviceService.GetAllDevice(continuationToken: continuationToken,filterDeviceType: "LoRa Concentrator", pageSize: 100);
                //var pouet = this.concentratorService.GetAllDeviceConcentrator(result, this.Url);

                twins.AddRange(result.Items);

                totalTwinConcentrators = result.TotalItems;
                continuationToken = result.NextPage;

            } while (totalTwinConcentrators > twins.Count);

            return twins;
        }

        private async Task CreateOrUpdateConcentrator(Twin twin)
        {
            var concentrator = this.mapper.Map<Concentrator>(twin);

            var concentratorEntity = await this.concentratorRepository.GetByIdAsync(concentrator.Id);
            if (concentratorEntity == null)
            {
                await this.concentratorRepository.InsertAsync(concentrator);
            }
            else
            {
                if (concentratorEntity.Version >= concentratorEntity.Version) return;

                _ = this.mapper.Map(concentrator, concentratorEntity);
                this.concentratorRepository.Update(concentratorEntity);
            }
        }
    }
}

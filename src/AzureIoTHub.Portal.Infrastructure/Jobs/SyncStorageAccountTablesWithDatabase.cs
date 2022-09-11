// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Jobs
{
    using Application.Abstractions.Services;
    using AutoMapper;
    using Domain;
    using Domain.Entities;
    using Domain.Repositories;
    using Quartz;

    public class SyncStorageAccountTablesWithDatabase : IJob
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceTagRepository deviceTagRepository;
        private readonly IDeviceTagService deviceTagService;

        public SyncStorageAccountTablesWithDatabase(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceTagRepository deviceTagRepository,
            IDeviceTagService deviceTagService
            )
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceTagRepository = deviceTagRepository;
            this.deviceTagService = deviceTagService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var tags = this.deviceTagService.GetAllTags();

            foreach (var deviceTag in tags)
            {
                if (!await this.deviceTagRepository.DeviceTagExists(deviceTag.Name))
                {
                    _ = await this.deviceTagRepository.CreateDeviceTag(this.mapper.Map<DeviceTag>(deviceTag));
                }
            }

            await this.unitOfWork.SaveAsync();
        }
    }
}

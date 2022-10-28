// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Models.v10;
    using Domain.Repositories;
    using Domain.Exceptions;
    using Managers;
    using Infrastructure;
    using AzureIoTHub.Portal.Domain.Entities;
    using Domain;
    using Mappers;
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;

    public class DeviceService : DeviceServiceBase<DeviceDetails>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceTwinMapper<DeviceListItem, DeviceDetails> deviceTwinMapper,
            PortalDbContext portalDbContext)
            : base(portalDbContext, externalDevicesService, deviceTagService, deviceModelImageManager, deviceTwinMapper)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceRepository = deviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public override async Task<DeviceDetails> GetDevice(string deviceId)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device with id {deviceId} doesn't exist");
            }

            var deviceDto = this.mapper.Map<DeviceDetails>(deviceEntity);

            deviceDto.ImageUrl = this.deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);

            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
        }

        protected override async Task<DeviceDetails> CreateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = this.mapper.Map<Device>(device);

            await this.deviceRepository.InsertAsync(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task<DeviceDetails> UpdateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(device.DeviceID);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device {device.DeviceID} doesn't exist");
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            _ = this.mapper.Map(device, deviceEntity);

            this.deviceRepository.Update(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task DeleteDeviceInDatabase(string deviceId)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId);

            if (deviceEntity == null)
            {
                return;
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            this.deviceRepository.Delete(deviceId);

            await this.unitOfWork.SaveAsync();
        }

        public override async Task<Stream> ExportDeviceList()
        {
            Console.WriteLine("Call to ExportDeviceList() on DeviceService");
            Console.Clear();

            var tags = this.deviceTagService.GetAllTagsNames();

            var query = this.portalDbContext.Devices
                .Include(device => device.Tags);

            var devices = await query.ToListAsync();

            var header = "Id,Name,DeviceModelId,IsEnabled,Version";
            foreach (var tag in tags)
            {
                header += $",TAG:{tag}";
            }

            var stream = new FileStream("test2.csv",FileMode.OpenOrCreate);
            //var writer = new StreamWriter(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.WriteLine(header);
            Console.WriteLine(header);

            foreach (var device in devices)
            {
                var line =$"{device.Id},{device.Name},{device.DeviceModelId},{device.IsEnabled},{device.Version}";
                foreach (var tag in tags)
                {
                    var value = device.Tags.Where(x => x.Name == tag).Select(x => x.Value).SingleOrDefault();
                    line += $",{value}";
                }
                Console.WriteLine(line);
                writer.WriteLine(line);
            }

            return stream;
        }
    }
}

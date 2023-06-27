namespace IoTHub.Portal.Infrastructure.Azure.Mappers.Resolvers
{
    using AutoMapper;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Infrastructure.Azure.Helpers;
    using IoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices.Shared;
    using System;

    public class DeviceImageUrlValueResolver : IValueResolver<Twin, DeviceDetailsDto, Uri>
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceImageUrlValueResolver(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public Uri Resolve(Twin source, DeviceDetailsDto destination, Uri destMember, ResolutionContext context)
        {
            return this.deviceModelImageManager.ComputeImageUri(DeviceHelper.RetrieveTagValue(source, nameof(destination.ModelId))!);
        }
    }
}

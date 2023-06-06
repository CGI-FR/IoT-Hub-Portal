// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services.AWS
{
    using System.Threading.Tasks;
    using Amazon.IoT.Model;
    using Amazon.IotData.Model;
    using AzureIoTHub.Portal.Models.v10;

    public interface IAWSExternalDeviceService
    {
        Task<DescribeThingResponse> GetDevice(string deviceName);

        Task<CreateThingResponse> CreateDevice(CreateThingRequest device);

        Task<UpdateThingResponse> UpdateDevice(UpdateThingRequest device);

        Task<DeleteThingResponse> DeleteDevice(DeleteThingRequest device);

        Task<GetThingShadowResponse> GetDeviceShadow(string deviceName);

        Task<UpdateThingShadowResponse> UpdateDeviceShadow(UpdateThingShadowRequest shadow);
        Task<int> GetEdgeDeviceNbDevices(IoTEdgeDevice device);

        Task<bool?> IsEdgeThingType(DescribeThingTypeResponse thingType);

    }
}

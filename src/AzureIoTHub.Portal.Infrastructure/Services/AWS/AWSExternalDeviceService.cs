// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.IotData;
    using Amazon.IotData.Model;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using Microsoft.Extensions.Logging;

    public class AWSExternalDeviceService : IAWSExternalDeviceService
    {
        private readonly ILogger<AWSExternalDeviceService> log;
        private readonly IAmazonIoT amazonIotClient;
        private readonly IAmazonIotData amazonIotDataClient;

        public AWSExternalDeviceService(ILogger<AWSExternalDeviceService> log
            , IAmazonIoT amazonIoTClient
            , IAmazonIotData amazonIotDataClient)
        {
            this.log = log;
            this.amazonIotClient = amazonIoTClient;
            this.amazonIotDataClient = amazonIotDataClient;
        }

        public async Task<DescribeThingResponse> GetDevice(string deviceName)
        {
            var deviceRequest = new DescribeThingRequest { ThingName = deviceName };

            var deviceResponse = await this.amazonIotClient.DescribeThingAsync(deviceRequest);

            if (deviceResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to get the thing with device name : {deviceName} due to an error in the Amazon IoT API : {deviceResponse.HttpStatusCode}");
            }

            return deviceResponse;
        }

        public async Task<CreateThingResponse> CreateDevice(CreateThingRequest device)
        {
            var thingResponse = await this.amazonIotClient.CreateThingAsync(device);

            if (thingResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to create the thing with id {device.ThingName} due to an error in the Amazon IoT API : {thingResponse.HttpStatusCode}");
            }

            return thingResponse;
        }

        public async Task<UpdateThingResponse> UpdateDevice(UpdateThingRequest device)
        {
            var thingResponse = await this.amazonIotClient.UpdateThingAsync(device);

            if (thingResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to update the thing with id {device.ThingName} due to an error in the Amazon IoT API : {thingResponse.HttpStatusCode}");
            }

            return thingResponse;
        }

        public async Task<GetThingShadowResponse> GetDeviceShadow(string deviceName)
        {
            var shadowRequest = new GetThingShadowRequest { ThingName = deviceName };

            var shadowResponse = await this.amazonIotDataClient.GetThingShadowAsync(shadowRequest);

            if (shadowResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to get the thing shadow with device name : {deviceName} due to an error in the Amazon IoT API : {shadowResponse.HttpStatusCode}");
            }

            return shadowResponse;
        }

        public async Task<UpdateThingShadowResponse> UpdateDeviceShadow(UpdateThingShadowRequest shadow)
        {
            var shadowResponse = await this.amazonIotDataClient.UpdateThingShadowAsync(shadow);

            if (shadowResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to create/update the thing shadow with device name : {shadow.ThingName} due to an error in the Amazon IoT API : {shadowResponse.HttpStatusCode}");
            }

            return shadowResponse;
        }
    }
}

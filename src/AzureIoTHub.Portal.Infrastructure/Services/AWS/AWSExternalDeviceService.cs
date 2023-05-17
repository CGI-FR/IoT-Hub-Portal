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

        public async Task<CreateThingResponse> CreateDevice(CreateThingRequest device)
        {
            var thingResponse = await this.amazonIotClient.CreateThingAsync(device);

            if (thingResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException("The creation of the thing failed due to an error in the Amazon IoT API.");
            }

            return thingResponse;
        }

        public async Task<UpdateThingShadowResponse> UpdateDeviceShadow(UpdateThingShadowRequest shadow)
        {
            var shadowResponse = await this.amazonIotDataClient.UpdateThingShadowAsync(shadow);

            if (shadowResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException("The creation/update of the thing shadow failed due to an error in the Amazon IoT API.");
            }

            return shadowResponse;
        }
    }
}

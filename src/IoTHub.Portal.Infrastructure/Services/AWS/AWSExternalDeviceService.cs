// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.IotData;
    using Amazon.IotData.Model;
    using IoTHub.Portal.Application.Services.AWS;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Models.v10;

    public class AWSExternalDeviceService : IAWSExternalDeviceService
    {
        private readonly IAmazonIoT amazonIotClient;
        private readonly IAmazonIotData amazonIotDataClient;
        private readonly IAmazonGreengrassV2 amazonGreenGrasss;

        public AWSExternalDeviceService(
            IAmazonIoT amazonIoTClient,
            IAmazonIotData amazonIotDataClient,
            IAmazonGreengrassV2 amazonGreenGrasss)
        {
            this.amazonIotClient = amazonIoTClient;
            this.amazonIotDataClient = amazonIotDataClient;
            this.amazonGreenGrasss = amazonGreenGrasss;
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
                throw new InternalServerErrorException($"Unable to create the thing with device name : {device.ThingName} due to an error in the Amazon IoT API : {thingResponse.HttpStatusCode}");
            }

            return thingResponse;
        }

        public async Task<UpdateThingResponse> UpdateDevice(UpdateThingRequest device)
        {
            var thingResponse = await this.amazonIotClient.UpdateThingAsync(device);

            if (thingResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to update the thing with device name : {device.ThingName} due to an error in the Amazon IoT API : {thingResponse.HttpStatusCode}");
            }

            return thingResponse;
        }

        public async Task<DeleteThingResponse> DeleteDevice(DeleteThingRequest device)
        {
            //Retreive all thing princpals and detach it before deleting the thing
            var principals = await this.amazonIotClient.ListThingPrincipalsAsync(new ListThingPrincipalsRequest
            {
                NextToken = string.Empty,
                ThingName = device.ThingName
            });

            if (principals.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to retreive Thing {device.ThingName} principals due to an error in the Amazon IoT API : {principals.HttpStatusCode}");

            }

            foreach (var principal in principals.Principals)
            {
                var detachPrincipal = await this.amazonIotClient.DetachThingPrincipalAsync(new DetachThingPrincipalRequest
                {
                    Principal = principal,
                    ThingName = device.ThingName
                });

                if (detachPrincipal.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InternalServerErrorException($"Unable to detach Thing {device.ThingName} principal due to an error in the Amazon IoT API : {detachPrincipal.HttpStatusCode}");

                }
            }

            //Delete the thing type before detaching the princiapl
            var deleteResponse = await this.amazonIotClient.DeleteThingAsync(device);

            if (deleteResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to delete the thing with device name : {device.ThingName} due to an error in the Amazon IoT API : {deleteResponse.HttpStatusCode}");
            }

            return deleteResponse;
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

        public async Task<int> GetEdgeDeviceNbDevices(IoTEdgeDevice device)
        {
            var listClientDevices = await this.amazonGreenGrasss.ListClientDevicesAssociatedWithCoreDeviceAsync(
                new Amazon.GreengrassV2.Model.ListClientDevicesAssociatedWithCoreDeviceRequest
                {
                    CoreDeviceThingName = device.DeviceName
                });
            return listClientDevices.HttpStatusCode != System.Net.HttpStatusCode.OK
                ? throw new InternalServerErrorException($"Can not list Client Devices Associated to {device.DeviceName} Core Device due to an error in the Amazon IoT API.")
                : listClientDevices.AssociatedClientDevices.Count;
        }

        public async Task<bool?> IsEdgeThingType(DescribeThingTypeResponse thingType)
        {
            var response = await this.amazonIotClient.ListTagsForResourceAsync(new ListTagsForResourceRequest
            {
                ResourceArn = thingType.ThingTypeArn
            });

            do
            {
                if (response == null || !response.Tags.Any())
                {
                    return null;
                }

                var iotEdgeTag = response.Tags.Where(c => c.Key.Equals("iotEdge", StringComparison.OrdinalIgnoreCase));

                if (!iotEdgeTag.Any())
                {
                    response = await this.amazonIotClient.ListTagsForResourceAsync(new ListTagsForResourceRequest
                    {
                        ResourceArn = thingType.ThingTypeArn,
                        NextToken = response.NextToken
                    });

                    continue;
                }

                return bool.TryParse(iotEdgeTag.Single().Value, out var result) ? result : null;

            } while (true);
        }
    }
}

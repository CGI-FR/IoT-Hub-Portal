// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.AWS.Services
{
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.IotData;
    using Amazon.IotData.Model;
    using Amazon.SecretsManager;
    using Amazon.SecretsManager.Model;
    using AutoMapper;
    using IoTHub.Portal;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Shared;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Shared.Models.v10;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Device = Microsoft.Azure.Devices.Device;
    using ListTagsForResourceRequest = Amazon.IoT.Model.ListTagsForResourceRequest;
    using ResourceAlreadyExistsException = Amazon.IoT.Model.ResourceAlreadyExistsException;
    using ResourceNotFoundException = Amazon.IoT.Model.ResourceNotFoundException;
    using Tag = Amazon.IoT.Model.Tag;

    public class ExternalDeviceService : IExternalDeviceService
    {
        private const string PrivateKeyKey = "_private-key";
        private const string PublicKeyKey = "_public-key";
        private const string CertificateKey = "_certificate";

        private readonly ConfigHandler configHandler;
        private readonly IMapper mapper;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonGreengrassV2 greengrass;
        private readonly IAmazonSecretsManager amazonSecretsManager;
        private readonly IAmazonIotData amazonIotData;
        private readonly ILogger<ExternalDeviceService> logger;

        public ExternalDeviceService(
            ConfigHandler configHandler,
            IMapper mapper,
            IAmazonIoT amazonIoTClient,
            IAmazonGreengrassV2 greengrass,
            IAmazonSecretsManager amazonSecretsManager,
            ILogger<ExternalDeviceService> logger,
            IAmazonIotData amazonIotData)
        {
            this.configHandler = configHandler;
            this.mapper = mapper;
            this.amazonIoTClient = amazonIoTClient;
            this.greengrass = greengrass;
            this.amazonSecretsManager = amazonSecretsManager;
            this.logger = logger;
            this.amazonIotData = amazonIotData;
        }

        public async Task<ExternalDeviceModelDto> CreateDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            try
            {
                var createThingTypeRequest = mapper.Map<CreateThingTypeRequest>(deviceModel);

                createThingTypeRequest.Tags.Add(new Tag
                {
                    Key = "iotEdge",
                    Value = "False"
                });

                var response = await amazonIoTClient.CreateThingTypeAsync(createThingTypeRequest);
                await CreateDynamicGroupForThingType(response.ThingTypeName);

                deviceModel.Id = response.ThingTypeId;

                return deviceModel;
            }
            catch (ResourceAlreadyExistsException e)
            {
                throw new Domain.Exceptions.ResourceAlreadyExistsException($"Device Model already exists. Unable to create the device model {deviceModel.Name}: {e.Message}", e);
            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException($"Unable to create the device model {deviceModel.Name}: {e.Message}", e);
            }
        }

        public Task<Twin> CreateNewTwinFromDeviceId(string deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteDevice(string deviceId)
        {
            try
            {
                _ = await greengrass.DeleteCoreDeviceAsync(new DeleteCoreDeviceRequest
                {
                    CoreDeviceThingName = deviceId,
                });
            }
            catch (Amazon.GreengrassV2.Model.ResourceNotFoundException)
            {
                logger.LogWarning("Unable to Delete Core Device because it doesn't exist");
            }

            try
            {
                _ = await amazonIoTClient.DeleteThingAsync(new DeleteThingRequest
                {
                    ThingName = deviceId
                });
            }
            catch (ResourceNotFoundException e)
            {
                logger.LogWarning("Unable to delete the thing because it doesn't exist", e);
            }

        }

        public async Task DeleteDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            try
            {
                var deprecated = new DeprecateThingTypeRequest()
                {
                    ThingTypeName = deviceModel.Name,
                    UndoDeprecate = false
                };

                _ = await amazonIoTClient.DeprecateThingTypeAsync(deprecated);

                try
                {
                    _ = await amazonIoTClient.DeleteDynamicThingGroupAsync(new DeleteDynamicThingGroupRequest
                    {
                        ThingGroupName = deviceModel.Name
                    });
                }
                catch (ResourceNotFoundException e)
                {
                    throw new Domain.Exceptions.ResourceNotFoundException($"Thing Group not found. Unable to delete the device model {deviceModel.Name}: {e.Message}", e);
                }
                catch (AmazonIoTException e)
                {
                    throw new Domain.Exceptions.InternalServerErrorException($"Unable to delete the device model {deviceModel.Name}: {e.Message}", e);
                }

            }
            catch (ResourceNotFoundException e)
            {
                throw new Domain.Exceptions.ResourceNotFoundException($"Thing type not Found. Unable to deprecate the device model {deviceModel.Name}: {e.Message}", e);
            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException($"Unable to delete the device model {deviceModel.Name}: {e.Message}", e);
            }
        }

        public async Task<bool?> IsEdgeDeviceModel(ExternalDeviceModelDto deviceModel)
        {

            try
            {
                var thingType = await amazonIoTClient.DescribeThingTypeAsync(new DescribeThingTypeRequest()
                {
                    ThingTypeName = deviceModel.Name
                });

                try
                {
                    var response = await amazonIoTClient.ListTagsForResourceAsync(new ListTagsForResourceRequest
                    {
                        ResourceArn = thingType.ThingTypeArn
                    });

                    do
                    {
                        if (response == null || !response.Tags.Any())
                            return null;

                        var iotEdgeTag = response.Tags.Where(c => c.Key.Equals("iotEdge", StringComparison.OrdinalIgnoreCase));

                        if (!iotEdgeTag.Any())
                        {
                            try
                            {
                                response = await amazonIoTClient.ListTagsForResourceAsync(new ListTagsForResourceRequest
                                {
                                    ResourceArn = thingType.ThingTypeArn,
                                    NextToken = response.NextToken
                                });

                                continue;
                            }
                            catch (AmazonIoTException e)
                            {
                                throw new Domain.Exceptions.InternalServerErrorException($"Unable to list tags thing type {deviceModel.Name}: {e.Message}", e);

                            }
                        }

                        return bool.TryParse(iotEdgeTag.Single().Value, out var result) ? result : null;

                    } while (true);
                }
                catch (AmazonIoTException e)
                {
                    throw new Domain.Exceptions.InternalServerErrorException($"Unable to list tags thing type {deviceModel.Name}: {e.Message}", e);

                }
            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException($"Unable to describe the the thing type {deviceModel.Name}: {e.Message}", e);

            }

        }

        public Task<CloudToDeviceMethodResult> ExecuteC2DMethod(string deviceId, CloudToDeviceMethod method)
        {
            throw new NotImplementedException();
        }

        public Task<CloudToDeviceMethodResult> ExecuteCustomCommandC2DMethod(string deviceId, string moduleName, CloudToDeviceMethod method)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationResult<Twin>> GetAllDevice(string? continuationToken = null, string? filterDeviceType = null, string? excludeDeviceType = null, string? searchText = null, bool? searchStatus = null, bool? searchState = null, Dictionary<string, string>? searchTags = null, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<DescribeThingResponse>> GetAllThing()
        {
            var things = new List<DescribeThingResponse>();

            var marker = string.Empty;

            do
            {
                var request = new ListThingsRequest
                {
                    Marker = marker
                };

                try
                {
                    var response = await amazonIoTClient.ListThingsAsync(request);

                    foreach (var requestDescribeThing in response.Things.Select(thing => new DescribeThingRequest { ThingName = thing.ThingName }))
                    {
                        try
                        {
                            things.Add(await amazonIoTClient.DescribeThingAsync(requestDescribeThing));
                        }
                        catch (AmazonIoTException e)
                        {
                            logger.LogWarning($"Cannot import device '{requestDescribeThing.ThingName}' due to an error in the Amazon IoT API.", e);

                            continue;
                        }
                    }

                    marker = response.NextMarker;
                }
                catch (AmazonIoTException e)
                {
                    throw new Domain.Exceptions.InternalServerErrorException($"Unable to list thing types : {e.Message}", e);
                }
            }
            while (!string.IsNullOrEmpty(marker));

            return things;
        }

        public Task<PaginationResult<Twin>> GetAllEdgeDevice(string? continuationToken = null, string? searchText = null, bool? searchStatus = null, string? searchType = null, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAllGatewayID()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetConcentratorsCount()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetConnectedDevicesCount()
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetConnectedEdgeDevicesCount()
        {
            try
            {
                var coreDevices = await greengrass.ListCoreDevicesAsync(new ListCoreDevicesRequest
                {
                    NextToken = string.Empty
                });

                return coreDevices.CoreDevices.Where(c => c.Status == CoreDeviceStatus.HEALTHY).Count();
            }
            catch (AmazonGreengrassV2Exception e)
            {
                throw new Domain.Exceptions.InternalServerErrorException($"Unable to List Core Devices due to an error in the Amazon IoT API.", e);
            }
        }

        public Task<Device> GetDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetDevicesCount()
        {
            var deviceCount = await Count();

            return deviceCount["DeviceCount"];
        }

        public async Task<int> GetEdgeDevicesCount()
        {
            var edgeDeviceCount = await Count();

            return edgeDeviceCount["EdgeDeviceCount"];
        }

        private async Task<Dictionary<string, int>> Count()
        {
            var deviceCount = 0;
            var edgeDeviceCount = 0;

            var devices = await GetAllThing();
            var dico = new Dictionary<string, int>();

            bool? isEdge;

            foreach (var device in devices)
            {
                try
                {
                    isEdge = await IsEdgeDeviceModel(mapper.Map<ExternalDeviceModelDto>(device));
                }
                catch (AmazonIoTException e)
                {
                    logger.LogWarning($"Cannot import device '{device.ThingName}' due to an error retrieving thing shadow in the Amazon IoT Data API.", e);
                    continue;
                }

                // Cannot know if the thing type was created for an iotEdge or not, so skipping...
                if (!isEdge.HasValue)
                    continue;

                if (isEdge != true)
                {
                    try
                    {
                        _ = await amazonIotData.GetThingShadowAsync(new GetThingShadowRequest
                        {
                            ThingName = device.ThingName,
                        });
                    }
                    catch (ResourceNotFoundException e)
                    {
                        logger.LogInformation($"Cannot import device '{device.ThingName}' since it doesn't have related classic thing shadow", e);
                        continue;
                    }
                    catch (AmazonIotDataException e)
                    {
                        logger.LogWarning($"Cannot import device '{device.ThingName}' due to an error retrieving thing shadow in the Amazon IoT Data API.", e);
                        continue;
                    }
                    deviceCount++;
                }
                else
                {
                    edgeDeviceCount++;
                }
            }

            dico.Add("DeviceCount", deviceCount);
            dico.Add("EdgeDeviceCount", edgeDeviceCount);

            return dico;

        }
        public Task<IEnumerable<string>> GetDevicesToExport()
        {
            throw new NotImplementedException();
        }

        public Task<Twin> GetDeviceTwin(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<Twin> GetDeviceTwinWithEdgeHubModule(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<Twin> GetDeviceTwinWithModule(string deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task<DeviceCredentialsDto> GetEdgeDeviceCredentials(IoTEdgeDeviceDto device)
        {
            DeviceCredentialsDto deviceCredentials;

            try
            {
                deviceCredentials = await GetDeviceCredentialsFromSecretsManager(device.DeviceName);
            }
            catch (Amazon.SecretsManager.Model.ResourceNotFoundException)
            {
                var createCertificateTuple = await GenerateCertificate(device.DeviceName);

                foreach (var item in configHandler.AWSGreengrassRequiredRoles)
                {
                    _ = await amazonIoTClient.AttachPolicyAsync(new AttachPolicyRequest
                    {
                        PolicyName = item,
                        Target = createCertificateTuple.Item2
                    });
                }

                return createCertificateTuple.Item1;
            }

            return deviceCredentials;
        }

        public Task<IEnumerable<IoTEdgeDeviceLogDto>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModuleDto edgeModule)
        {
            throw new NotImplementedException();
        }

        public async Task<DeviceCredentialsDto> GetDeviceCredentials(IDeviceDetails device)
        {
            try
            {
                return await GetDeviceCredentialsFromSecretsManager(device.DeviceName);
            }
            catch (Amazon.SecretsManager.Model.ResourceNotFoundException)
            {
                var deviceCredentialsTuple = await GenerateCertificate(device.DeviceName);

                return deviceCredentialsTuple.Item1;
            }
        }

        public async Task<ConfigItemDto> RetrieveLastConfiguration(IoTEdgeDeviceDto ioTEdgeDevice)
        {
            try
            {
                var coreDevice = await greengrass.GetCoreDeviceAsync(new GetCoreDeviceRequest
                {
                    CoreDeviceThingName = ioTEdgeDevice.DeviceName
                });

                return new ConfigItemDto
                {
                    Name = coreDevice.CoreDeviceThingName,
                    DateCreation = coreDevice.LastStatusUpdateTimestamp,
                    Status = coreDevice.Status
                };
            }
            catch (Amazon.GreengrassV2.Model.ResourceNotFoundException)
            {
                return null!;
            }
        }

        public Task<Device> UpdateDevice(Device device)
        {
            throw new NotImplementedException();
        }

        public Task<Twin> UpdateDeviceTwin(Twin twin)
        {
            throw new NotImplementedException();
        }

        private async Task CreateDynamicGroupForThingType(string thingTypeName)
        {
            try
            {
                var dynamicThingGroup = new DescribeThingGroupRequest
                {
                    ThingGroupName = thingTypeName
                };

                _ = await amazonIoTClient.DescribeThingGroupAsync(dynamicThingGroup);
            }
            catch (ResourceNotFoundException)
            {
                _ = await amazonIoTClient.CreateDynamicThingGroupAsync(new CreateDynamicThingGroupRequest
                {
                    ThingGroupName = thingTypeName,
                    QueryString = $"thingTypeName: {thingTypeName}"
                });
            }
        }

        private async Task<Tuple<DeviceCredentialsDto, string>> GenerateCertificate(string deviceName)
        {

            try
            {
                var response = await amazonIoTClient.CreateKeysAndCertificateAsync(true);

                try
                {
                    _ = await amazonIoTClient.AttachThingPrincipalAsync(deviceName, response.CertificateArn);

                    _ = await CreatePrivateKeySecret(deviceName, response.KeyPair.PrivateKey);
                    _ = await CreatePublicKeySecret(deviceName, response.KeyPair.PublicKey);
                    _ = await CreateCertificateSecret(deviceName, response.CertificatePem);
                    _ = await AttachCertificateToThing(deviceName, response.CertificateArn);

                    return new Tuple<DeviceCredentialsDto, string>(new DeviceCredentialsDto
                    {
                        AuthenticationMode = AuthenticationMode.Certificate,
                        CertificateCredentials = new CertificateCredentialsDto
                        {
                            CertificatePem = response.CertificatePem,
                            PrivateKey = response.KeyPair.PrivateKey,
                            PublicKey = response.KeyPair.PublicKey,
                        }
                    }, response.CertificateArn);

                }
                catch (AmazonIoTException e)
                {
                    throw new Domain.Exceptions.InternalServerErrorException("Unable to Attach Thing Principal due to an error in the Amazon IoT API.", e);
                }
            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to Create Keys and Certificate due to an error in the Amazon IoT API.", e);
            }
        }

        private async Task<AttachThingPrincipalResponse> AttachCertificateToThing(string deviceName, string certificateArn)
        {
            try
            {
                return await amazonIoTClient.AttachThingPrincipalAsync(deviceName, certificateArn);
            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to Attach Thing Principal due to an error in the Amazon IoT API.", e);
            }
        }

        private async Task<CreateSecretResponse> CreatePrivateKeySecret(string deviceName, string privateKey)
        {
            try
            {
                var request = new CreateSecretRequest
                {
                    Name = deviceName + PrivateKeyKey,
                    Description = "Private key for the certificate of device " + deviceName,
                    SecretString = privateKey
                };
                return await amazonSecretsManager.CreateSecretAsync(request);
            }
            catch (AmazonSecretsManagerException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to Create Private Secret due to an error in the Amazon IoT API.", e);
            }
        }

        private async Task<CreateSecretResponse> CreatePublicKeySecret(string deviceName, string privateKey)
        {
            try
            {
                var request = new CreateSecretRequest
                {
                    Name = deviceName + PublicKeyKey,
                    Description = "Public key for the certificate of device " + deviceName,
                    SecretString = privateKey
                };
                return await amazonSecretsManager.CreateSecretAsync(request);
            }
            catch (AmazonSecretsManagerException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to Create Public Secret due to an error in the Amazon IoT API.", e);
            }
        }

        private async Task<CreateSecretResponse> CreateCertificateSecret(string deviceName, string certificatePem)
        {
            try
            {
                var request = new CreateSecretRequest
                {
                    Name = deviceName + CertificateKey,
                    Description = "Certificate for the certificate of device " + deviceName,
                    SecretString = certificatePem
                };
                return await amazonSecretsManager.CreateSecretAsync(request);
            }
            catch (AmazonSecretsManagerException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to Create Certificate due to an error in the Amazon IoT API.", e);

            }
        }


        private async Task<DeviceCredentialsDto> GetDeviceCredentialsFromSecretsManager(string deviceName)
        {
            var certificate = await GetSecret(deviceName + CertificateKey);
            var privateKey = await GetSecret(deviceName + PrivateKeyKey);
            var publicKey = await GetSecret(deviceName + PublicKeyKey);
            return new DeviceCredentialsDto
            {
                AuthenticationMode = AuthenticationMode.Certificate,
                CertificateCredentials = new CertificateCredentialsDto
                {
                    CertificatePem = certificate.SecretString,
                    PrivateKey = privateKey.SecretString,
                    PublicKey = publicKey.SecretString,
                }
            };
        }

        private async Task<GetSecretValueResponse> GetSecret(string secretId)
        {
            return await amazonSecretsManager.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secretId,
            });
        }

        public async Task<string> CreateEnrollementScript(string template, IoTEdgeDeviceDto device)
        {
            try
            {
                var iotDataEndpointResponse = await amazonIoTClient.DescribeEndpointAsync(new DescribeEndpointRequest
                {
                    EndpointType = "iot:Data-ATS"
                });

                var credentialProviderEndpointResponse = await amazonIoTClient.DescribeEndpointAsync(new DescribeEndpointRequest
                {
                    EndpointType = "iot:CredentialProvider"
                });

                var credentials = await GetEdgeDeviceCredentials(device);

                return template.Replace("%DATA_ENDPOINT%", iotDataEndpointResponse!.EndpointAddress, StringComparison.OrdinalIgnoreCase)
                   .Replace("%CREDENTIALS_ENDPOINT%", credentialProviderEndpointResponse.EndpointAddress, StringComparison.OrdinalIgnoreCase)
                   .Replace("%CERTIFICATE%", credentials.CertificateCredentials.CertificatePem, StringComparison.OrdinalIgnoreCase)
                   .Replace("%PRIVATE_KEY%", credentials.CertificateCredentials.PrivateKey, StringComparison.OrdinalIgnoreCase)
                   .Replace("%REGION%", configHandler.AWSRegion, StringComparison.OrdinalIgnoreCase)
                   .Replace("%GREENGRASSCORETOKENEXCHANGEROLEALIAS%", configHandler.AWSGreengrassCoreTokenExchangeRoleAliasName, StringComparison.OrdinalIgnoreCase)
                   .Replace("%THING_NAME%", device.DeviceName, StringComparison.OrdinalIgnoreCase)
                   .ReplaceLineEndings();
            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to Describe Endpoint due to an error in the Amazon IoT API.", e);
            }
        }

        public Task<BulkRegistryOperationResult> CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin, DeviceStatus isEnabled)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateEdgeDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveDeviceCredentials(IoTEdgeDeviceDto device)
        {
            try
            {
                var request = new ListThingPrincipalsRequest
                {
                    ThingName = device.DeviceName
                };

                do
                {

                    var principalResponse = await amazonIoTClient.ListThingPrincipalsAsync(request);

                    if (!principalResponse.Principals.Any())
                        break;

                    foreach (var item in principalResponse.Principals)
                    {
                        await RemoveGreengrassCertificateFromPrincipal(device, item);
                    }

                    request.NextToken = principalResponse.NextToken;

                }
                while (true);
            }
            catch (AmazonIoTException e)
            {
                logger.LogWarning("Unable to List Thing principal due to an error in the Amazon IoT API.", e);
            }
        }


        private async Task RemoveGreengrassCertificateFromPrincipal(IoTEdgeDeviceDto device, string principalId)
        {
            foreach (var item in configHandler.AWSGreengrassRequiredRoles)
            {
                try
                {
                    _ = await amazonIoTClient.AttachPolicyAsync(new AttachPolicyRequest
                    {
                        PolicyName = item,
                        Target = principalId
                    });
                }
                catch (AmazonIoTException e)
                {
                    throw new Domain.Exceptions.InternalServerErrorException("Unable to attach policy due to an error in the Amazon IoT API.", e);
                }
            }

            try
            {
                _ = await amazonIoTClient.DetachThingPrincipalAsync(device.DeviceName, principalId);

            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to detach thing Principal due to an error in the Amazon IoT API.", e);

            }

            await DeleteSecret(device.DeviceName + PublicKeyKey);
            await DeleteSecret(device.DeviceName + PrivateKeyKey);
            await DeleteSecret(device.DeviceName + CertificateKey);

            var awsPricipalCertRegex = new Regex("/arn:aws:iot:([a-z0-9-]*):(\\d*):cert\\/([0-9a-fA-F]*)/gm");

            var matches = awsPricipalCertRegex.Match(principalId);

            if (!matches.Success)
                return;

            var certificateId = matches.Captures[2].Value;

            try
            {
                _ = await amazonIoTClient.UpdateCertificateAsync(certificateId, CertificateStatus.REGISTER_INACTIVE);

            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to update certificate due to an error in the Amazon IoT API.", e);
            }
            try
            {
                _ = await amazonIoTClient.DeleteCertificateAsync(certificateId);
            }
            catch (AmazonIoTException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to delete certificate due to an error in the Amazon IoT API.", e);
            }
        }

        private async Task DeleteSecret(string secretId)
        {
            try
            {
                _ = await amazonSecretsManager.DeleteSecretAsync(new DeleteSecretRequest
                {
                    ForceDeleteWithoutRecovery = true,
                    SecretId = secretId,
                });
            }
            catch (AmazonSecretsManagerException e)
            {
                throw new Domain.Exceptions.InternalServerErrorException("Unable to delete secret due to an error in the Amazon IoT API.", e);
            }
        }
    }
}

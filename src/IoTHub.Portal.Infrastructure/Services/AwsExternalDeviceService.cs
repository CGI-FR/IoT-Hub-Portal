// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.SecretsManager;
    using Amazon.SecretsManager.Model;
    using AutoMapper;
    using IoTHub.Portal;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Shared;
    using IoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Shared.Models.v10;
    using ResourceAlreadyExistsException = Amazon.IoT.Model.ResourceAlreadyExistsException;
    using ResourceNotFoundException = Amazon.IoT.Model.ResourceNotFoundException;
    using Tag = Amazon.IoT.Model.Tag;

    public class AwsExternalDeviceService : IExternalDeviceService
    {
        private const string PrivateKeyKey = "_private-key";
        private const string PublicKeyKey = "_public-key";
        private const string CertificateKey = "_certificate";

        private readonly ConfigHandler configHandler;
        private readonly IMapper mapper;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonGreengrassV2 greengrass;
        private readonly IAmazonSecretsManager amazonSecretsManager;


        public AwsExternalDeviceService(
            ConfigHandler configHandler,
            IMapper mapper,
            IAmazonIoT amazonIoTClient,
            IAmazonGreengrassV2 greengrass,
            IAmazonSecretsManager amazonSecretsManager)
        {
            this.configHandler = configHandler;
            this.mapper = mapper;
            this.amazonIoTClient = amazonIoTClient;
            this.greengrass = greengrass;
            this.amazonSecretsManager = amazonSecretsManager;
        }

        public async Task<ExternalDeviceModelDto> CreateDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            try
            {
                var createThingTypeRequest = this.mapper.Map<CreateThingTypeRequest>(deviceModel);

                createThingTypeRequest.Tags.Add(new Tag
                {
                    Key = "iotEdge",
                    Value = "False"
                });

                var response = await this.amazonIoTClient.CreateThingTypeAsync(createThingTypeRequest);
                await this.CreateDynamicGroupForThingType(response.ThingTypeName);

                deviceModel.Id = response.ThingTypeId;

                return deviceModel;
            }
            catch (ResourceAlreadyExistsException e)
            {
                throw new Domain.Exceptions.ResourceAlreadyExistsException($"Unable to create the device model {deviceModel.Name}: {e.Message}", e);
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
                _ = await this.greengrass.DeleteCoreDeviceAsync(new DeleteCoreDeviceRequest
                {
                    CoreDeviceThingName = deviceId,
                });
            }
            catch (Amazon.GreengrassV2.Model.ResourceNotFoundException) { }

            try
            {
                _ = await this.amazonIoTClient.DeleteThingAsync(new DeleteThingRequest
                {
                    ThingName = deviceId
                });
            }
            catch (Amazon.IoT.Model.ResourceNotFoundException) { }
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

                _ = await this.amazonIoTClient.DeprecateThingTypeAsync(deprecated);

                _ = await this.amazonIoTClient.DeleteDynamicThingGroupAsync(new DeleteDynamicThingGroupRequest
                {
                    ThingGroupName = deviceModel.Name
                });
            }
            catch (ResourceNotFoundException e)
            {
                throw new Domain.Exceptions.ResourceNotFoundException($"Unable to delete the device model {deviceModel.Name}: {e.Message}", e);
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

        public Task<int> GetConnectedEdgeDevicesCount()
        {
            throw new NotImplementedException();
        }

        public Task<Device> GetDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetDevicesCount()
        {
            throw new NotImplementedException();
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

        public async Task<DeviceCredentials> GetEdgeDeviceCredentials(IoTEdgeDevice device)
        {
            DeviceCredentials deviceCredentials;

            try
            {
                deviceCredentials = await GetDeviceCredentialsFromSecretsManager(device.DeviceName);
            }
            catch (Amazon.SecretsManager.Model.ResourceNotFoundException)
            {
                var createCertificateTuple = await GenerateCertificate(device.DeviceName);

                foreach (var item in this.configHandler.AWSGreengrassRequiredRoles)
                {
                    _ = await this.amazonIoTClient.AttachPolicyAsync(new AttachPolicyRequest
                    {
                        PolicyName = item,
                        Target = createCertificateTuple.Item2
                    });
                }

                return createCertificateTuple.Item1;
            }

            return deviceCredentials;
        }

        public Task<IEnumerable<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule edgeModule)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetEdgeDevicesCount()
        {
            throw new NotImplementedException();
        }

        public async Task<DeviceCredentials> GetDeviceCredentials(string deviceName)
        {
            try
            {
                return await GetDeviceCredentialsFromSecretsManager(deviceName);
            }
            catch (Amazon.SecretsManager.Model.ResourceNotFoundException)
            {
                var deviceCredentialsTuple = await GenerateCertificate(deviceName);

                return deviceCredentialsTuple.Item1;
            }
        }

        public async Task<ConfigItem> RetrieveLastConfiguration(IoTEdgeDevice ioTEdgeDevice)
        {
            try
            {
                var coreDevice = await this.greengrass.GetCoreDeviceAsync(new GetCoreDeviceRequest
                {
                    CoreDeviceThingName = ioTEdgeDevice.DeviceName
                });

                return new ConfigItem
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

                _ = await this.amazonIoTClient.DescribeThingGroupAsync(dynamicThingGroup);
            }
            catch (ResourceNotFoundException)
            {
                _ = await this.amazonIoTClient.CreateDynamicThingGroupAsync(new CreateDynamicThingGroupRequest
                {
                    ThingGroupName = thingTypeName,
                    QueryString = $"thingTypeName: {thingTypeName}"
                });
            }
        }

        private async Task<Tuple<DeviceCredentials, string>> GenerateCertificate(string deviceName)
        {
            var response = await this.amazonIoTClient.CreateKeysAndCertificateAsync(true);

            _ = await this.amazonIoTClient.AttachThingPrincipalAsync(deviceName, response.CertificateArn);

            _ = await CreatePrivateKeySecret(deviceName, response.KeyPair.PrivateKey);
            _ = await CreatePublicKeySecret(deviceName, response.KeyPair.PublicKey);
            _ = await CreateCertificateSecret(deviceName, response.CertificatePem);
            _ = await AttachCertificateToThing(deviceName, response.CertificateArn);

            return new Tuple<DeviceCredentials, string>(new DeviceCredentials
            {
                AuthenticationMode = AuthenticationMode.Certificate,
                CertificateCredentials = new CertificateCredentials
                {
                    CertificatePem = response.CertificatePem,
                    PrivateKey = response.KeyPair.PrivateKey,
                    PublicKey = response.KeyPair.PublicKey,
                }
            }, response.CertificateArn);
        }

        private async Task<AttachThingPrincipalResponse> AttachCertificateToThing(string deviceName, string certificateArn)
        {
            return await this.amazonIoTClient.AttachThingPrincipalAsync(deviceName, certificateArn);
        }

        private async Task<CreateSecretResponse> CreatePrivateKeySecret(string deviceName, string privateKey)
        {
            var request = new CreateSecretRequest
            {
                Name = deviceName + PrivateKeyKey,
                Description = "Private key for the certificate of device " + deviceName,
                SecretString = privateKey
            };
            return await this.amazonSecretsManager.CreateSecretAsync(request);
        }

        private async Task<CreateSecretResponse> CreatePublicKeySecret(string deviceName, string privateKey)
        {
            var request = new CreateSecretRequest
            {
                Name = deviceName + PublicKeyKey,
                Description = "Public key for the certificate of device " + deviceName,
                SecretString = privateKey
            };
            return await this.amazonSecretsManager.CreateSecretAsync(request);
        }

        private async Task<CreateSecretResponse> CreateCertificateSecret(string deviceName, string certificatePem)
        {
            var request = new CreateSecretRequest
            {
                Name = deviceName + CertificateKey,
                Description = "Certificate for the certificate of device " + deviceName,
                SecretString = certificatePem
            };
            return await this.amazonSecretsManager.CreateSecretAsync(request);
        }


        private async Task<DeviceCredentials> GetDeviceCredentialsFromSecretsManager(string deviceName)
        {
            var certificate = await GetSecret(deviceName + CertificateKey);
            var privateKey = await GetSecret(deviceName + PrivateKeyKey);
            var publicKey = await GetSecret(deviceName + PublicKeyKey);
            return new DeviceCredentials
            {
                AuthenticationMode = AuthenticationMode.Certificate,
                CertificateCredentials = new CertificateCredentials
                {
                    CertificatePem = certificate.SecretString,
                    PrivateKey = privateKey.SecretString,
                    PublicKey = publicKey.SecretString,
                }
            };
        }

        private async Task<GetSecretValueResponse> GetSecret(string secretId)
        {
            return await this.amazonSecretsManager.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secretId,
            });
        }

        public async Task<string> CreateEnrollementScript(string template, Domain.Entities.EdgeDevice device)
        {
            var iotDataEndpointResponse = await this.amazonIoTClient.DescribeEndpointAsync(new DescribeEndpointRequest
            {
                EndpointType = "iot:Data-ATS"
            });

            var credentialProviderEndpointResponse = await this.amazonIoTClient.DescribeEndpointAsync(new DescribeEndpointRequest
            {
                EndpointType = "iot:CredentialProvider"
            });

            var credentials = await this.GetDeviceCredentials(device.Name);

            return template.Replace("%DATA_ENDPOINT%", iotDataEndpointResponse!.EndpointAddress, StringComparison.OrdinalIgnoreCase)
               .Replace("%CREDENTIALS_ENDPOINT%", credentialProviderEndpointResponse.EndpointAddress, StringComparison.OrdinalIgnoreCase)
               .Replace("%CERTIFICATE%", credentials.CertificateCredentials.CertificatePem, StringComparison.OrdinalIgnoreCase)
               .Replace("%PRIVATE_KEY%", credentials.CertificateCredentials.PrivateKey, StringComparison.OrdinalIgnoreCase)
               .Replace("%REGION%", this.configHandler.AWSRegion, StringComparison.OrdinalIgnoreCase)
               .Replace("%GREENGRASSCORETOKENEXCHANGEROLEALIAS%", this.configHandler.AWSGreengrassCoreTokenExchangeRoleAliasName, StringComparison.OrdinalIgnoreCase)
               .Replace("%THING_NAME%", device.Name, StringComparison.OrdinalIgnoreCase)
               .ReplaceLineEndings();
        }

        public Task<BulkRegistryOperationResult> CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin, DeviceStatus isEnabled)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateEdgeDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveDeviceCredentials(IoTEdgeDevice device)
        {
            var request = new ListThingPrincipalsRequest
            {
                ThingName = device.DeviceName
            };

            do
            {
                var principalResponse = await this.amazonIoTClient.ListThingPrincipalsAsync(request);

                if (!principalResponse.Principals.Any())
                {
                    break;
                }

                foreach (var item in principalResponse.Principals)
                {
                    await RemoveGreengrassCertificateFromPrincipal(device, item);
                }

                request.NextToken = principalResponse.NextToken;
            }
            while (true);
        }


        private async Task RemoveGreengrassCertificateFromPrincipal(IoTEdgeDevice device, string principalId)
        {
            foreach (var item in this.configHandler.AWSGreengrassRequiredRoles)
            {
                _ = await this.amazonIoTClient.AttachPolicyAsync(new AttachPolicyRequest
                {
                    PolicyName = item,
                    Target = principalId
                });
            }

            _ = await this.amazonIoTClient.DetachThingPrincipalAsync(device.DeviceName, principalId);

            _ = await this.amazonSecretsManager.DeleteSecretAsync(new DeleteSecretRequest
            {
                ForceDeleteWithoutRecovery = true,
                SecretId = device.DeviceName + PublicKeyKey,
            });

            _ = await this.amazonSecretsManager.DeleteSecretAsync(new DeleteSecretRequest
            {
                ForceDeleteWithoutRecovery = true,
                SecretId = device.DeviceName + PrivateKeyKey,
            });

            _ = await this.amazonSecretsManager.DeleteSecretAsync(new DeleteSecretRequest
            {
                ForceDeleteWithoutRecovery = true,
                SecretId = device.DeviceName + CertificateKey,
            });

            var awsPricipalCertRegex = new Regex("/arn:aws:iot:([a-z0-9-]*):(\\d*):cert\\/([0-9a-fA-F]*)/gm");

            var matches = awsPricipalCertRegex.Match(principalId);

            if (!matches.Success)
            {
                return;
            }

            var certificateId = matches.Captures[2].Value;

            _ = await this.amazonIoTClient.UpdateCertificateAsync(certificateId, CertificateStatus.REGISTER_INACTIVE);
            _ = await this.amazonIoTClient.DeleteCertificateAsync(certificateId);
        }
    }
}

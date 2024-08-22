// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.SecretsManager;
    using Amazon.SecretsManager.Model;
    using AutoFixture;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Shared;
    using IoTHub.Portal.Infrastructure.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Amazon.IotData;
    using ListTagsForResourceRequest = Amazon.IoT.Model.ListTagsForResourceRequest;
    using ListTagsForResourceResponse = Amazon.IoT.Model.ListTagsForResourceResponse;
    using IoTHub.Portal.Domain.Entities;
    using System.Net;
    using Shared.Models.v1._0;
    using Tag = Amazon.IoT.Model.Tag;
    using Device = Portal.Domain.Entities.Device;

    [TestFixture]
    public class AwsExternalDeviceServiceTests : BackendUnitTest
    {
        private Mock<IAmazonIoT> mockAmazonIot;
        private Mock<IAmazonGreengrassV2> mockGreengrassV2;
        private Mock<IAmazonSecretsManager> mockSecretsManager;
        private Mock<IAmazonIotData> mocAmazonIotData;
        private Mock<ConfigHandler> mockConfigHandler;

        private IExternalDeviceService externalDeviceService;

        public override void Setup()
        {
            base.Setup();

            this.mockAmazonIot = MockRepository.Create<IAmazonIoT>();
            this.mockGreengrassV2 = MockRepository.Create<IAmazonGreengrassV2>();
            this.mockSecretsManager = MockRepository.Create<IAmazonSecretsManager>();
            this.mocAmazonIotData = MockRepository.Create<IAmazonIotData>();
            this.mockConfigHandler = MockRepository.Create<ConfigHandler>();


            _ = ServiceCollection.AddSingleton(this.mockAmazonIot.Object);
            _ = ServiceCollection.AddSingleton(this.mockGreengrassV2.Object);
            _ = ServiceCollection.AddSingleton(this.mockSecretsManager.Object);
            _ = ServiceCollection.AddSingleton(this.mocAmazonIotData.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigHandler.Object);
            _ = ServiceCollection.AddSingleton<IExternalDeviceService, AwsExternalDeviceService>();


            Services = ServiceCollection.BuildServiceProvider();

            this.externalDeviceService = Services.GetRequiredService<IExternalDeviceService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateDeviceModel_NonExistingThingType_ThingTypeCreated()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            var createThingTypeResponse = new CreateThingTypeResponse
            {
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = externalDeviceModelDto.Name
            };

            _ = this.mockAmazonIot.Setup(e => e.CreateThingTypeAsync(It.Is<CreateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createThingTypeResponse);

            _ = this.mockAmazonIot.Setup(c => c.DescribeThingGroupAsync(It.Is<DescribeThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException(It.IsAny<string>()));

            _ = this.mockAmazonIot.Setup(c => c.CreateDynamicThingGroupAsync(It.Is<CreateDynamicThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)
                                                                                                                        && d.QueryString.Equals($"thingTypeName: {externalDeviceModelDto.Name}", StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateDynamicThingGroupResponse());

            // Act
            var result = await this.externalDeviceService.CreateDeviceModel(externalDeviceModelDto);

            // Assert
            _ = result.Id.Should().Be(createThingTypeResponse.ThingTypeId);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceModel_ExistingThingType_ResourceAlreadyExistsExceptionIsThrown()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.CreateThingTypeAsync(It.Is<CreateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceAlreadyExistsException(Fixture.Create<string>()));

            // Act
            var act = () => this.externalDeviceService.CreateDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<Portal.Domain.Exceptions.ResourceAlreadyExistsException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_ExistingThingType_ThingTypeDepcrecated()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.DeprecateThingTypeAsync(It.Is<DeprecateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase) && !c.UndoDeprecate), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeprecateThingTypeResponse());

            _ = this.mockAmazonIot.Setup(c => c.DeleteDynamicThingGroupAsync(It.Is<DeleteDynamicThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new DeleteDynamicThingGroupResponse());

            // Act
            await this.externalDeviceService.DeleteDeviceModel(externalDeviceModelDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_NonExistingThingType_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.DeprecateThingTypeAsync(It.Is<DeprecateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase) && !c.UndoDeprecate), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException(Fixture.Create<string>()));

            // Act
            var act = () => this.externalDeviceService.DeleteDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<Portal.Domain.Exceptions.ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceShouldDeleteAWSCoreDevice()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockGreengrassV2.Setup(c => c.DeleteCoreDeviceAsync(It.Is<DeleteCoreDeviceRequest>(r => r.CoreDeviceThingName == deviceId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Fixture.Create<DeleteCoreDeviceResponse>());

            _ = this.mockAmazonIot.Setup(c => c.DeleteThingAsync(It.Is<DeleteThingRequest>(r => r.ThingName == deviceId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Fixture.Create<DeleteThingResponse>());

            // Act
            await this.externalDeviceService.DeleteDevice(deviceId);

            // Assert
            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenThingNotExistsAndCoreReturnBadRequestDeviceDeleteShouldPass()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockGreengrassV2.Setup(c => c.DeleteCoreDeviceAsync(It.Is<DeleteCoreDeviceRequest>(r => r.CoreDeviceThingName == deviceId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteCoreDeviceResponse { HttpStatusCode = HttpStatusCode.BadRequest });

            _ = this.mockAmazonIot.Setup(c => c.DeleteThingAsync(It.Is<DeleteThingRequest>(r => r.ThingName == deviceId), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException(Fixture.Create<string>()));

            // Act
            await this.externalDeviceService.DeleteDevice(deviceId);

            // Assert
            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenThingExistsAndCoreDoesNotExistDeviceDeleteShouldPass()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockGreengrassV2.Setup(c => c.DeleteCoreDeviceAsync(It.Is<DeleteCoreDeviceRequest>(r => r.CoreDeviceThingName == deviceId), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.GreengrassV2.Model.ResourceNotFoundException(Fixture.Create<string>()));

            _ = this.mockAmazonIot.Setup(c => c.DeleteThingAsync(It.Is<DeleteThingRequest>(r => r.ThingName == deviceId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteThingResponse { HttpStatusCode = HttpStatusCode.NoContent });

            // Act
            await this.externalDeviceService.DeleteDevice(deviceId);

            // Assert

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenThingNotExistsAndCoreDoesExistDeviceDeleteShouldPass()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockGreengrassV2.Setup(c => c.DeleteCoreDeviceAsync(It.Is<DeleteCoreDeviceRequest>(r => r.CoreDeviceThingName == deviceId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteCoreDeviceResponse { HttpStatusCode = HttpStatusCode.NoContent });

            _ = this.mockAmazonIot.Setup(c => c.DeleteThingAsync(It.Is<DeleteThingRequest>(r => r.ThingName == deviceId), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException(Fixture.Create<string>()));

            // Act
            await this.externalDeviceService.DeleteDevice(deviceId);

            // Assert

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceWithTwinShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.CreateDeviceWithTwin(Fixture.Create<string>(), Fixture.Create<bool>(), Fixture.Create<Twin>(), Fixture.Create<DeviceStatus>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task CreateNewTwinFromDeviceIdShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.CreateNewTwinFromDeviceId(Fixture.Create<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task ExecuteC2DMethodShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.ExecuteC2DMethod(Fixture.Create<string>(), Fixture.Create<CloudToDeviceMethod >());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task ExecuteCustomCommandC2DMethodShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.ExecuteCustomCommandC2DMethod(Fixture.Create<string>(), Fixture.Create<string>(), Fixture.Create<CloudToDeviceMethod >());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetAllDeviceShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetAllDevice();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetAllEdgeDeviceShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetAllEdgeDevice();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetAllGatewayIDShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetAllGatewayID();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetConcentratorsCountShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetConcentratorsCount();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetConnectedDevicesCountShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetConnectedDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetConnectedEdgeDevicesCountShouldReturnConnectedEdgeDeviceNumber()
        {
            //Arrange
            _ = this.mockGreengrassV2.Setup(greengrass => greengrass.ListCoreDevicesAsync(It.IsAny<ListCoreDevicesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListCoreDevicesResponse());
            // Act
            var act = () => this.externalDeviceService.GetConnectedEdgeDevicesCount();

            // Assert
            _ = act.Should().NotBeNull();
        }

        [Test]
        public async Task GetDeviceShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDevice(It.IsAny<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public Task GetDevicesCountShouldReturnDeviceNumber()
        {
            //Arrange
            _ = this.mockAmazonIot.Setup(client => client.ListThingsAsync(It.IsAny<ListThingsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListThingsResponse());

            _ = this.mockAmazonIot.Setup(client => client.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingTypeResponse());

            _ = this.mockAmazonIot.Setup(client => client.ListTagsForResourceAsync(It.IsAny<ListTagsForResourceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListTagsForResourceResponse()
                {
                    Tags = new List<Amazon.IoT.Model.Tag>()
                    {
                        new Amazon.IoT.Model.Tag()
                        {
                            Key = "iotEdge",
                            Value = "False"
                        }
                    }
                });

            // Act
            var act = () => this.externalDeviceService.GetDevicesCount();

            // Assert
            _ = act.Should().NotBeNull();
            return Task.CompletedTask;
        }

        [Test]
        public async Task GetDevicesToExportShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDevicesToExport();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDeviceTwinShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDeviceTwin(It.IsAny<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDeviceTwinWithEdgeHubModuleShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDeviceTwinWithEdgeHubModule(It.IsAny<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDeviceTwinWithModuleShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDeviceTwinWithModule(It.IsAny<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetEdgeDeviceLogsShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetEdgeDeviceLogs(It.IsAny<string>(), Fixture.Create<IoTEdgeModule>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetEdgeDevicesCountShouldThrowNotImplementedException()
        {
            //Arrange
            _ = this.mockAmazonIot.Setup(client => client.ListThingsAsync(It.IsAny<ListThingsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListThingsResponse());

            _ = this.mockAmazonIot.Setup(client => client.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingTypeResponse());

            _ = this.mockAmazonIot.Setup(client => client.ListTagsForResourceAsync(It.IsAny<ListTagsForResourceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListTagsForResourceResponse()
                {
                    Tags = new List<Amazon.IoT.Model.Tag>()
                    {
                        new Amazon.IoT.Model.Tag()
                        {
                            Key = "iotEdge",
                            Value = "True"
                        }
                    }
                });

            // Act
            var act = () => this.externalDeviceService.GetDevicesCount();

            // Assert
            _ = act.Should().NotBeNull();
        }

        [Test]
        public async Task RetrieveLastConfigurationShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.RetrieveLastConfiguration(Fixture.Create<IoTEdgeDevice>());

            // Assert
            _ = act.Should().NotBeNull();
        }

        [Test]
        public async Task UpdateDeviceShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.UpdateDevice(new Microsoft.Azure.Devices.Device());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task CreateEdgeDeviceShouldThrowNotImplementedException()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            // Act
            var act = () => this.externalDeviceService.CreateEdgeDevice(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task UpdateDeviceTwinShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.UpdateDeviceTwin(Fixture.Create<Twin>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetEdgeDeviceCredentialsShouldReturnExisitingDeviceCredentials()
        {
            // Arrange
            var device = Fixture.Create<IoTEdgeDevice>();

            _ = this.mockSecretsManager.Setup(c => c.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetSecretValueResponse());

            // Act
            var result = this.externalDeviceService.GetEdgeDeviceCredentials(device);

            // Assert
            _ = result.Should().NotBeNull();

        }

        [Test]
        public async Task GetEdgeDeviceCredentialsShouldCreateAndReturnDeviceCredentials()
        {
            // Arrange
            var device = Fixture.Create<IoTEdgeDevice>();

            _ = this.mockSecretsManager.Setup(c => c.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.SecretsManager.Model.ResourceNotFoundException("Resource Not found"));


            _ = this.mockAmazonIot.Setup(c => c.AttachPolicyAsync(It.IsAny<AttachPolicyRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AttachPolicyResponse());

            _ = this.mockAmazonIot.Setup(c => c.CreateKeysAndCertificateAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateKeysAndCertificateResponse());

            _ = this.mockAmazonIot.Setup(c => c.AttachThingPrincipalAsync(It.IsAny<AttachThingPrincipalRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AttachThingPrincipalResponse());

            _ = this.mockSecretsManager.Setup(c => c.CreateSecretAsync(It.IsAny<CreateSecretRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateSecretResponse());

            // Act
            var result = this.externalDeviceService.GetEdgeDeviceCredentials(device);

            // Assert
            _ = result.Should().NotBeNull();

        }

        [Test]
        public async Task GetDeviceCredentialsShouldReturnExisitingDeviceCredentials()
        {
            // Arrange
            var device = Fixture.Create<DeviceDetails>();

            _ = this.mockSecretsManager.Setup(c => c.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetSecretValueResponse());

            // Act
            var result = this.externalDeviceService.GetDeviceCredentials(device);

            // Assert
            _ = result.Should().NotBeNull();

        }

        [Test]
        public async Task GetDeviceCredentialsShouldCreateAndReturnDeviceCredentials()
        {
            // Arrange
            var device = Fixture.Create<DeviceDetails>();

            _ = this.mockSecretsManager.Setup(c => c.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.SecretsManager.Model.ResourceNotFoundException("Resource Not found"));


            _ = this.mockAmazonIot.Setup(c => c.AttachPolicyAsync(It.IsAny<AttachPolicyRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AttachPolicyResponse());

            _ = this.mockAmazonIot.Setup(c => c.CreateKeysAndCertificateAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateKeysAndCertificateResponse());

            _ = this.mockAmazonIot.Setup(c => c.AttachThingPrincipalAsync(It.IsAny<AttachThingPrincipalRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AttachThingPrincipalResponse());

            _ = this.mockSecretsManager.Setup(c => c.CreateSecretAsync(It.IsAny<CreateSecretRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateSecretResponse());

            // Act
            var result = this.externalDeviceService.GetDeviceCredentials(device);

            // Assert
            _ = result.Should().NotBeNull();
        }

        [Test]
        public async Task RetriveEdgeDeviceLastDeploymentShouldRetuenLastDeployment()
        {
            //Arrange
            var device = Fixture.Create<IoTEdgeDevice>();

            _ = this.mockGreengrassV2.Setup(c => c.GetCoreDeviceAsync(It.IsAny<GetCoreDeviceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoreDeviceResponse());

            // Act
            var result = this.externalDeviceService.RetrieveLastConfiguration(device);

            // Assert
            _ = result.Should().NotBeNull();
        }

        [Test]
        public async Task RemoveDeviceCredentialsShouldRemoveDeviceCredentials()
        {
            // Arrange
            var device = Fixture.Create<IoTEdgeDevice>();

            _ = this.mockAmazonIot.Setup(c => c.ListThingPrincipalsAsync(It.IsAny<ListThingPrincipalsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListThingPrincipalsResponse
                {
                    Principals = Fixture.Create<List<string>>()
                });

            _ = this.mockAmazonIot.Setup(c => c.DetachPolicyAsync(It.IsAny<DetachPolicyRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DetachPolicyResponse());

            _ = this.mockAmazonIot.Setup(c => c.DetachThingPrincipalAsync(It.IsAny<DetachThingPrincipalRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DetachThingPrincipalResponse());

            _ = this.mockSecretsManager.Setup(c => c.DeleteSecretAsync(It.IsAny<DeleteSecretRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteSecretResponse());

            _ = this.mockAmazonIot.Setup(c => c.UpdateCertificateAsync(It.IsAny<UpdateCertificateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateCertificateResponse());

            _ = this.mockAmazonIot.Setup(c => c.DeleteCertificateAsync(It.IsAny<DeleteCertificateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteCertificateResponse());

            // Act
            var result = this.externalDeviceService.RemoveDeviceCredentials(device);

            // Assert
            _ = result.Should().NotBeNull();
        }

        [Test]
        public async Task IsEdgeThingTypeReturnTrue()
        {
            // Arrange
            var thingType = new DescribeThingTypeResponse()
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            _ = this.mockAmazonIot.Setup(client => client.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingTypeResponse
                {
                    ThingTypeId = thingType.ThingTypeId,
                    ThingTypeName = thingType.ThingTypeName,
                    ThingTypeArn = thingType.ThingTypeArn,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIot.Setup(client => client.ListTagsForResourceAsync(It.Is<ListTagsForResourceRequest>(c => c.ResourceArn == thingType.ThingTypeArn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListTagsForResourceResponse
                {
                    NextToken = Fixture.Create<string>(),
                    Tags = new List<Tag>
                    {
                                    new Tag
                                    {
                                        Key = "iotEdge",
                                        Value = "true"
                                    }
                    }
                });

            //Act
            var result = await this.externalDeviceService.IsEdgeDeviceModel(this.Mapper.Map<ExternalDeviceModelDto>(thingType));

            //Assert
            _ = result.Should().BeTrue();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task IsNotEdgeThingTypeReturnFalse()
        {
            // Arrange
            var thingType = new DescribeThingTypeResponse()
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            _ = this.mockAmazonIot.Setup(client => client.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingTypeResponse
                {
                    ThingTypeId = thingType.ThingTypeId,
                    ThingTypeName = thingType.ThingTypeName,
                    ThingTypeArn = thingType.ThingTypeArn,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIot.Setup(client => client.ListTagsForResourceAsync(It.Is<ListTagsForResourceRequest>(c => c.ResourceArn == thingType.ThingTypeArn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListTagsForResourceResponse
                {
                    NextToken = Fixture.Create<string>(),
                    Tags = new List<Tag>
                    {
                                    new Tag
                                    {
                                        Key = "iotEdge",
                                        Value = "false"
                                    }
                    }
                });

            //Act
            var result = await this.externalDeviceService.IsEdgeDeviceModel(this.Mapper.Map<ExternalDeviceModelDto>(thingType));

            //Assert
            _ = result.Should().BeFalse();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task EdgeThingTypeNullReturnNull()
        {
            // Arrange
            var thingType = new DescribeThingTypeResponse()
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            _ = this.mockAmazonIot.Setup(client => client.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingTypeResponse
                {
                    ThingTypeId = thingType.ThingTypeId,
                    ThingTypeName = thingType.ThingTypeName,
                    ThingTypeArn = thingType.ThingTypeArn,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIot.Setup(client => client.ListTagsForResourceAsync(It.Is<ListTagsForResourceRequest>(c => c.ResourceArn == thingType.ThingTypeArn), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ListTagsForResourceResponse)null);

            //Act
            var result = await this.externalDeviceService.IsEdgeDeviceModel(this.Mapper.Map<ExternalDeviceModelDto>(thingType));

            //Assert
            _ = result.Should().BeNull();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task EdgeThingTypeNotBooleanReturnNull()
        {
            // Arrange
            var thingType = new DescribeThingTypeResponse()
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            _ = this.mockAmazonIot.Setup(client => client.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingTypeResponse
                {
                    ThingTypeId = thingType.ThingTypeId,
                    ThingTypeName = thingType.ThingTypeName,
                    ThingTypeArn = thingType.ThingTypeArn,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIot.Setup(client => client.ListTagsForResourceAsync(It.Is<ListTagsForResourceRequest>(c => c.ResourceArn == thingType.ThingTypeArn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListTagsForResourceResponse
                {
                    NextToken = Fixture.Create<string>(),
                    Tags = new List<Tag>
                    {
                                    new Tag
                                    {
                                        Key = "iotEdge",
                                        Value = "123"
                                    }
                    }
                });

            //Act
            var result = await this.externalDeviceService.IsEdgeDeviceModel(this.Mapper.Map<ExternalDeviceModelDto>(thingType));

            //Assert
            _ = result.Should().BeNull();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllThingShouldReturnsAllAWSThings()
        {

            //Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            var newDevice = new Device
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 1
            };

            var thingsListing = new ListThingsResponse
            {
                Things = new List<ThingAttribute>()
                {
                    new ThingAttribute
                    {
                        ThingName = newDevice.Name
                    }
                }
            };

            _ = this.mockAmazonIot.Setup(client => client.ListThingsAsync(It.IsAny<ListThingsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

            _ = this.mockAmazonIot.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = newDevice.Id,
                    ThingName = newDevice.Name,
                    ThingTypeName = newDevice.DeviceModel.Name,
                    Version = newDevice.Version,
                    HttpStatusCode = HttpStatusCode.OK
                });


            //Act
            var result = await this.externalDeviceService.GetAllThing();

            //Assert
            MockRepository.VerifyAll();
        }
    }
}

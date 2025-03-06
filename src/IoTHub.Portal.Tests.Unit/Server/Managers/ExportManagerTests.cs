// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Managers
{
    [TestFixture]
    public class ExportManagerTests : BackendUnitTest
    {
        private IExportManager exportManager;
        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private Mock<IDeviceService<DeviceDetails>> mockDeviceService;
        private Mock<IDeviceService<LoRaDeviceDetails>> mockLoraDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceModelPropertiesService> mockDeviceModelPropertiesService;
        private Mock<IDevicePropertyService> mockDevicePropertyService;
        private Mock<IOptions<LoRaWANOptions>> mockLoRaWANOptions;

        public override void Setup()
        {
            base.Setup();

            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceService = MockRepository.Create<IDeviceService<DeviceDetails>>();
            this.mockLoraDeviceService = MockRepository.Create<IDeviceService<LoRaDeviceDetails>>();
            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceModelPropertiesService = MockRepository.Create<IDeviceModelPropertiesService>();
            this.mockDevicePropertyService = MockRepository.Create<IDevicePropertyService>();
            this.mockLoRaWANOptions = MockRepository.Create<IOptions<LoRaWANOptions>>();

            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockLoraDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelPropertiesService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDevicePropertyService.Object);
            _ = ServiceCollection.AddSingleton(this.mockLoRaWANOptions.Object);

            _ = ServiceCollection.AddSingleton<IExportManager, ExportManager>();

            Services = ServiceCollection.BuildServiceProvider();

            this.exportManager = Services.GetRequiredService<IExportManager>();
        }

        [Test]
        public void ExportDeviceListLoRaDisabledShouldWriteStream()
        {
            // Arrange
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = false
                });

            using var fileStream = new MemoryStream();

            _ = this.mockExternalDeviceService.Setup(x => x.GetDevicesToExport())
            .ReturnsAsync(new List<string>()
                {
                    /*lang=json*/
                    "{ \"deviceId\": \"000001\", \"tags\": { \"deviceName\": \"DeviceExport01\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-1\"}, \"desired\": { \"Property1\": \"123\", \"Property2\": \"456\" }}",
                    /*lang=json*/
                    "{ \"deviceId\": \"000002\", \"tags\": { \"deviceName\": \"DeviceExport02\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-2\"}, \"desired\": { \"Property1\": \"789\", \"Property2\": \"000\" }}"
                });

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetAllPropertiesNames())
                .Returns(new List<string>() { "Property1", "Property2" });

            // Act + Assert
            Assert.AreEqual(0, fileStream.Length);
            _ = this.exportManager.ExportDeviceList(fileStream);
            fileStream.Position = 0;
            Assert.AreNotEqual(0, fileStream.Length);

            using var reader = new StreamReader(fileStream);
            var header = reader.ReadLine();
            _ = header.Split(",").Length.Should().Be(7);
            var content = reader.ReadToEnd();
            _ = content.TrimEnd().Split("\r\n").Length.Should().Be(2);
        }

        [Test]
        public void ExportDeviceListLoRaEnabledShouldWriteStreamAndDisplayLoRaSpecificField()
        {
            // Arrange
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = true
                });

            using var fileStream = new MemoryStream();

            _ = this.mockExternalDeviceService.Setup(x => x.GetDevicesToExport())
            .ReturnsAsync(new List<string>()
                {
                    /*lang=json*/
                    "{ \"deviceId\": \"000001\", \"tags\": { \"deviceName\": \"DeviceExport01\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-1\"}, \"desired\": { \"Property1\": \"123\", \"Property2\": \"456\" }}",
                    /*lang=json*/
                    "{ \"deviceId\": \"000002\", \"tags\": { \"deviceName\": \"DeviceExport02\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-2\"}, \"desired\": { \"Property1\": \"789\", \"Property2\": \"000\" }}"
                });

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetAllPropertiesNames())
                .Returns(new List<string>() { "Property1", "Property2" });

            // Act + Assert
            Assert.AreEqual(0, fileStream.Length);
            _ = this.exportManager.ExportDeviceList(fileStream);
            fileStream.Position = 0;
            Assert.AreNotEqual(0, fileStream.Length);

            using var reader = new StreamReader(fileStream);
            var header = reader.ReadLine();
            _ = header.Split(",").Length.Should().Be(14);
            var content = reader.ReadToEnd();
            _ = content.TrimEnd().Split("\r\n").Length.Should().Be(2);

            MockRepository.VerifyAll();
        }

        [Test]
        public void ExportTemplateFileLoRaDisabledShouldWriteStream()
        {
            // Arrange
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = false
                });

            using var fileStream = new MemoryStream();

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetAllPropertiesNames())
                .Returns(new List<string>() { "Property1", "Property2" });

            // Act + Assert
            Assert.AreEqual(0, fileStream.Length);
            _ = this.exportManager.ExportTemplateFile(fileStream);
            fileStream.Position = 0;
            Assert.AreNotEqual(0, fileStream.Length);

            using var reader = new StreamReader(fileStream);
            var content = reader.ReadToEnd();
            _ = content.TrimEnd().Split("\r\n").Length.Should().Be(1);
            _ = content.Split(",").Length.Should().Be(8);
        }

        [Test]
        public void ExportTemplateFileLoRaEnabledShouldWriteStreamAndDisplayLoRaSpecificField()
        {
            // Arrange
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = true
                });

            using var fileStream = new MemoryStream();

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetAllPropertiesNames())
                .Returns(new List<string>() { "Property1", "Property2" });

            // Act + Assert
            Assert.AreEqual(0, fileStream.Length);
            _ = this.exportManager.ExportTemplateFile(fileStream);
            fileStream.Position = 0;
            Assert.AreNotEqual(0, fileStream.Length);

            using var reader = new StreamReader(fileStream);
            var content = reader.ReadToEnd();
            _ = content.TrimEnd().Split("\r\n").Length.Should().Be(1);
            _ = content.Split(",").Length.Should().Be(14);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ImportDeviceListWrongFileFormatShouldThrowInternalServerErrorExceptionAsync()
        {
            // Arrange
            var input = Encoding.UTF8.GetBytes(Fixture.Create<string>());
            using var stream = new MemoryStream(input);

            // Act
            var act = () => this.exportManager.ImportDeviceList(stream);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ImportDeviceListCorrectFileNonExistingDevicesShouldCreateDevices()
        {
            // Arrange
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = true
                });

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetModelProperties(It.IsAny<string>()))
                .ReturnsAsync(new List<DeviceModelProperty>()
                {
                    new DeviceModelProperty()
                    {
                        Name = "Property1",
                        PropertyType = Models.DevicePropertyType.String
                    },
                    new DeviceModelProperty()
                    {
                        Name = "Property2",
                        PropertyType = Models.DevicePropertyType.String
                    },
                });

            _ = this.mockDeviceService.Setup(x => x.CheckIfDeviceExists(It.IsAny<string>()))
                .ReturnsAsync(false);

            _ = this.mockLoraDeviceService.Setup(x => x.CheckIfDeviceExists(It.IsAny<string>()))
                .ReturnsAsync(false);

            _ = this.mockDeviceService.Setup(x => x.CreateDevice(It.IsAny<DeviceDetails>()))
                .ReturnsAsync(new DeviceDetails());

            _ = this.mockLoraDeviceService.Setup(x => x.CreateDevice(It.IsAny<LoRaDeviceDetails>()))
                .ReturnsAsync(new LoRaDeviceDetails());

            _ = this.mockDevicePropertyService.Setup(x => x.SetProperties(It.IsAny<string>(), It.IsAny<IEnumerable<DevicePropertyValue>>()))
                .Returns(Task.CompletedTask);

            // Correct file format
            var textContent = new StringBuilder();
            _ = textContent.AppendLine("Id,Name,ModelId,TAG:supportLoRaFeatures,TAG:Tag1,TAG:Tag2,PROPERTY:Property1,PROPERTY:Property2,PROPERTY:AppKey,PROPERTY:AppEUI,PROPERTY:AppSKey,PROPERTY:NwkSKey,PROPERTY:DevAddr,PROPERTY:GatewayID,PROPERTY:Downlink,PROPERTY:ClassType,PROPERTY:PreferredWindow,PROPERTY:Deduplication,PROPERTY:RX1DROffset,PROPERTY:RX2DataRate,PROPERTY:RXDelay,PROPERTY:ABPRelaxMode,PROPERTY:SensorDecoder,PROPERTY:FCntUpStart,PROPERTY:FCntDownStart,PROPERTY:FCntResetCounter,PROPERTY:Supports32BitFCnt,PROPERTY:KeepAliveTimeout");
            _ = textContent.AppendLine("0000000000000001,ImportLoRa,dc1f171b-8e51-4c6d-a1c6-942b4a0f995b,true,Tag1-Value1,Tag2-Value1,,,AppKeyValue,AppEUIValue,,,,,true,C,1,Drop,,,1,,http://sensor-decoder-url/test,,,,,1");
            _ = textContent.AppendLine("0000000000000002,ImportNonLoRa,f8b7a67a-345d-463e-ae0e-eeb0f6d24e38,false,Tag1-Value2,Tag2-Value2,Property1Value,Property1Value,,,,,,,,,,,,,,,,,,,,");

            var bytes = Encoding.UTF8.GetBytes(textContent.ToString());
            using var stream = new MemoryStream(bytes);

            // Act
            var result = await this.exportManager.ImportDeviceList(stream);

            // Assert
            _ = result.Should().BeNullOrEmpty();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ImportDeviceListCorrectFileExistingDevicesShouldUpdateDevices()
        {
            // Arrange
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = true
                });

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetModelProperties(It.IsAny<string>()))
                .ReturnsAsync(new List<DeviceModelProperty>()
                {
                    new DeviceModelProperty()
                    {
                        Name = "Property1",
                        PropertyType = Models.DevicePropertyType.String
                    },
                    new DeviceModelProperty()
                    {
                        Name = "Property2",
                        PropertyType = Models.DevicePropertyType.String
                    },
                });

            _ = this.mockDeviceService.Setup(x => x.CheckIfDeviceExists(It.IsAny<string>()))
                .ReturnsAsync(true);

            _ = this.mockLoraDeviceService.Setup(x => x.CheckIfDeviceExists(It.IsAny<string>()))
                .ReturnsAsync(true);

            _ = this.mockDeviceService.Setup(x => x.UpdateDevice(It.IsAny<DeviceDetails>()))
                .ReturnsAsync(new DeviceDetails());

            _ = this.mockLoraDeviceService.Setup(x => x.UpdateDevice(It.IsAny<LoRaDeviceDetails>()))
                .ReturnsAsync(new LoRaDeviceDetails());

            _ = this.mockDevicePropertyService.Setup(x => x.SetProperties(It.IsAny<string>(), It.IsAny<IEnumerable<DevicePropertyValue>>()))
                .Returns(Task.CompletedTask);

            // Correct file format
            var textContent = new StringBuilder();
            _ = textContent.AppendLine("Id,Name,ModelId,TAG:supportLoRaFeatures,TAG:Tag1,TAG:Tag2,PROPERTY:Property1,PROPERTY:Property2,PROPERTY:AppKey,PROPERTY:AppEUI,PROPERTY:AppSKey,PROPERTY:NwkSKey,PROPERTY:DevAddr,PROPERTY:GatewayID,PROPERTY:Downlink,PROPERTY:ClassType,PROPERTY:PreferredWindow,PROPERTY:Deduplication,PROPERTY:RX1DROffset,PROPERTY:RX2DataRate,PROPERTY:RXDelay,PROPERTY:ABPRelaxMode,PROPERTY:SensorDecoder,PROPERTY:FCntUpStart,PROPERTY:FCntDownStart,PROPERTY:FCntResetCounter,PROPERTY:Supports32BitFCnt,PROPERTY:KeepAliveTimeout");
            _ = textContent.AppendLine("0000000000000001,ImportLoRa,dc1f171b-8e51-4c6d-a1c6-942b4a0f995b,true,Tag1-Value1,Tag2-Value1,,,AppKeyValue,AppEUIValue,,,,,true,C,1,Drop,,,1,,http://sensor-decoder-url/test,,,,,1");
            _ = textContent.AppendLine("0000000000000002,ImportNonLoRa,f8b7a67a-345d-463e-ae0e-eeb0f6d24e38,false,Tag1-Value2,Tag2-Value2,Property1Value,Property1Value,,,,,,,,,,,,,,,,,,,,");

            var bytes = Encoding.UTF8.GetBytes(textContent.ToString());
            using var stream = new MemoryStream(bytes);

            // Act
            var result = await this.exportManager.ImportDeviceList(stream);

            // Assert
            _ = result.Should().BeNullOrEmpty();

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ImportDeviceListCorrectFileMissingMandatoryFieldShouldDisplayErrors()
        {
            // Arrange
            var textContent = new StringBuilder();
            _ = textContent.AppendLine("Id,Name,ModelId,TAG:supportLoRaFeatures,TAG:Tag1,TAG:Tag2,PROPERTY:Property1,PROPERTY:Property2,PROPERTY:AppKey,PROPERTY:AppEUI,PROPERTY:AppSKey,PROPERTY:NwkSKey,PROPERTY:DevAddr,PROPERTY:GatewayID,PROPERTY:Downlink,PROPERTY:ClassType,PROPERTY:PreferredWindow,PROPERTY:Deduplication,PROPERTY:RX1DROffset,PROPERTY:RX2DataRate,PROPERTY:RXDelay,PROPERTY:ABPRelaxMode,PROPERTY:SensorDecoder,PROPERTY:FCntUpStart,PROPERTY:FCntDownStart,PROPERTY:FCntResetCounter,PROPERTY:Supports32BitFCnt,PROPERTY:KeepAliveTimeout");
            // Missing DeviceId
            _ = textContent.AppendLine(",ImportLoRa,dc1f171b-8e51-4c6d-a1c6-942b4a0f995b,true,Tag1-Value1,Tag2-Value1,,,AppKeyValue,AppEUIValue,,,,,true,C,1,Drop,,,1,,http://sensor-decoder-url/test,,,,,1");
            // Missing DeviceName
            _ = textContent.AppendLine("0000000000000002,,f8b7a67a-345d-463e-ae0e-eeb0f6d24e38,false,Tag1-Value2,Tag2-Value2,Property1Value,Property1Value,,,,,,,,,,,,,,,,,,,,");
            // Missing ModelId
            _ = textContent.AppendLine("0000000000000003,ImportNonLoRa,,false,Tag1-Value3,Tag2-Value3,Property1Value,Property1Value,,,,,,,,,,,,,,,,,,,,");

            var bytes = Encoding.UTF8.GetBytes(textContent.ToString());
            using var stream = new MemoryStream(bytes);

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = true
                });

            // Act
            var result = await this.exportManager.ImportDeviceList(stream);

            // Assert
            _ = result.Should().HaveCount(3);

            var resultArray = result.ToArray();

            _ = resultArray[0].LineNumber.Should().Be(1);
            _ = resultArray[0].DeviceId.Should().Be("-1");
            _ = resultArray[0].Message.Should().Be("The parameter Id cannot be null or empty");

            _ = resultArray[1].LineNumber.Should().Be(2);
            _ = resultArray[1].DeviceId.Should().Be("0000000000000002");
            _ = resultArray[1].Message.Should().Be("The parameter Name cannot be null or empty");

            _ = resultArray[2].LineNumber.Should().Be(3);
            _ = resultArray[2].DeviceId.Should().Be("0000000000000003");
            _ = resultArray[2].Message.Should().Be("The parameter ModelId cannot be null or empty");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ImportDeviceListCorrectFileIssueDuringCreationOrUpdateShouldDisplayError()
        {
            // Arrange
            _ = this.mockLoRaWANOptions.Setup(x => x.Value)
                .Returns(new LoRaWANOptions
                {
                    Enabled = true
                });

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames())
                .Returns(new List<string>() { "Tag1", "Tag2" });

            _ = this.mockDeviceModelPropertiesService.Setup(x => x.GetModelProperties(It.IsAny<string>()))
                .ReturnsAsync(new List<DeviceModelProperty>()
                {
                    new DeviceModelProperty()
                    {
                        Name = "Property1",
                        PropertyType = Models.DevicePropertyType.String
                    },
                    new DeviceModelProperty()
                    {
                        Name = "Property2",
                        PropertyType = Models.DevicePropertyType.String
                    },
                });

            _ = this.mockDeviceService.Setup(x => x.CheckIfDeviceExists(It.IsAny<string>()))
                .ReturnsAsync(true);

            _ = this.mockLoraDeviceService.Setup(x => x.CheckIfDeviceExists(It.IsAny<string>()))
                .ReturnsAsync(true);

            _ = this.mockDeviceService.Setup(x => x.UpdateDevice(It.IsAny<DeviceDetails>()))
                .Throws(new InternalServerErrorException("Unable to update the device."));

            _ = this.mockLoraDeviceService.Setup(x => x.UpdateDevice(It.IsAny<LoRaDeviceDetails>()))
                .Throws(new InternalServerErrorException("Unable to update the device"));

            // Correct file format
            var textContent = new StringBuilder();
            _ = textContent.AppendLine("Id,Name,ModelId,TAG:supportLoRaFeatures,TAG:Tag1,TAG:Tag2,PROPERTY:Property1,PROPERTY:Property2,PROPERTY:AppKey,PROPERTY:AppEUI,PROPERTY:AppSKey,PROPERTY:NwkSKey,PROPERTY:DevAddr,PROPERTY:GatewayID,PROPERTY:Downlink,PROPERTY:ClassType,PROPERTY:PreferredWindow,PROPERTY:Deduplication,PROPERTY:RX1DROffset,PROPERTY:RX2DataRate,PROPERTY:RXDelay,PROPERTY:ABPRelaxMode,PROPERTY:SensorDecoder,PROPERTY:FCntUpStart,PROPERTY:FCntDownStart,PROPERTY:FCntResetCounter,PROPERTY:Supports32BitFCnt,PROPERTY:KeepAliveTimeout");
            _ = textContent.AppendLine("0000000000000001,ImportLoRa,dc1f171b-8e51-4c6d-a1c6-942b4a0f995b,true,Tag1-Value1,Tag2-Value1,,,AppKeyValue,AppEUIValue,,,,,true,C,1,Drop,,,1,,http://sensor-decoder-url/test,,,,,1");
            _ = textContent.AppendLine("0000000000000002,ImportNonLoRa,f8b7a67a-345d-463e-ae0e-eeb0f6d24e38,false,Tag1-Value2,Tag2-Value2,Property1Value,Property1Value,,,,,,,,,,,,,,,,,,,,");

            var bytes = Encoding.UTF8.GetBytes(textContent.ToString());
            using var stream = new MemoryStream(bytes);

            // Act
            var result = await this.exportManager.ImportDeviceList(stream);

            // Assert
            _ = result.Should().HaveCount(2);
            MockRepository.VerifyAll();
        }
    }
}

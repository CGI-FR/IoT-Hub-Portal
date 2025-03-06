// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    [TestFixture]
    public class DevicesListPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;

        private readonly string apiBaseUrl = "api/devices";

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();
            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
        }

        [Test]
        public void DeviceListPageRendersCorrectly()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, IsConnected = true } }
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            cut.WaitForAssertion(() => cut.Find(".mud-expansion-panels .mud-expand-panel .mud-expand-panel-header .mud-expand-panel-text").TextContent.Should().Be("Search panel"));
            cut.WaitForAssertion(() => cut.Find(".mud-expansion-panels .mud-expand-panel").ClassList.Should().NotContain("Search panel should be collapsed"));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task WhenResetFilterButtonClickShouldClearFilters()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());


            // Act
            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForElement("#searchID").NodeValue = Guid.NewGuid().ToString();
            cut.WaitForElement("#searchStatusEnabled").Click();

            cut.WaitForElement("#resetSearch").Click();
            await Task.Delay(100);

            // Assert
            cut.WaitForAssertion(() => Assert.IsNull(cut.Find("#searchID").NodeValue));
            cut.WaitForAssertion(() => Assert.AreEqual("false", cut.Find("#searchStatusEnabled").Attributes["aria-checked"].Value));
            cut.WaitForAssertion(() => Assert.AreEqual("true", cut.Find("#searchStatusAll").Attributes["aria-checked"].Value));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenAddNewDeviceClickShouldNavigateToNewDevicePage()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();


            // Act
            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForElement("#addDeviceButton").Click();
            cut.WaitForAssertion(() => string.Equals("http://localhost/devices/new", mockNavigationManager.Uri, StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnRefreshShouldReloadDevices()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceListPage>();

            // Act
            cut.WaitForElement("#tableRefreshButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureDisableClickToItemShouldRedirectToDeviceDetailsPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId } }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/devices/{deviceId}"));
        }

        [Test]
        public void WhenLoraFeatureEnableClickToItemShouldRedirectToLoRaDeviceDetailsPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true } }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/devices/{deviceId}?isLora=true"));
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceTags()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LoadItemsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDevices()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task TypingSomeCharactersInTheAutocompleteShouldFilterTheDeviceModels()
        {
            // Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceClientService.Setup(service => service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new List<DeviceListItem>
                    {
                        new DeviceListItem()
                        {
                            DeviceID = Guid.NewGuid().ToString(),
                            DeviceName = Guid.NewGuid().ToString(),
                        },
                        new DeviceListItem()
                        {
                            DeviceID = Guid.NewGuid().ToString(),
                            DeviceName = Guid.NewGuid().ToString(),
                        }
                    }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto> { new DeviceModelDto
                    {
                        Name = "model_01",
                        Description = Guid.NewGuid().ToString(),
                    },
                    new DeviceModelDto
                    {
                        Name = "model_02",
                        Description = Guid.NewGuid().ToString(),
                    },
                    }
                });

            // Act
            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForElement($"#{nameof(DeviceModelDto.ModelId)}").Click();

            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll(".mud-input-helper-text").Count.Should().Be(2));

            var newModelList = await cut.Instance.Search("01");

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = newModelList.Count().Should().Be(2);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ShowDeviceTelemetry_Clicked_LoRaDeviceTelemetryDialogIsShown()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true, HasLoRaTelemetry = true } }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<LoRaDeviceTelemetryDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForElement($"#show-device-telemetry-{deviceId}").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeleteDevice_Clicked_DeleteDeviceDialogIsShown()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true, HasLoRaTelemetry = true } }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDevicePage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForElement($"#delete-device-{deviceId}").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ExportDevicesClickedShouldDownloadTheFile()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true } }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            using var emptyResult = new StringContent(string.Empty);

            _ = this.mockDeviceClientService.Setup(c => c.ExportDeviceList())
                .ReturnsAsync(emptyResult);

            cut.WaitForElement("#manageDevicesButtonToggle").Click();

            // Act
            popoverProvider.WaitForElement("#exportDevicesButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ExportTemplateClickedShouldDownloadTheFile()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true } }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            using var emptyResult = new StringContent(string.Empty);

            _ = this.mockDeviceClientService.Setup(c => c.ExportTemplateFile())
                .ReturnsAsync(emptyResult);

            cut.WaitForElement("#manageDevicesButtonToggle").Click();

            // Act
            popoverProvider.WaitForElement("#exportTemplateButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SearchDeviceModels_InputExisingDeviceModelName_DeviceModelReturned()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var deviceModels = Fixture.CreateMany<DeviceModelDto>(2).ToList();
            var expectedDeviceModel = deviceModels.First();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true } }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.Is<DeviceModelFilter>(x => expectedDeviceModel.Name.Equals(x.SearchText, StringComparison.Ordinal))))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = deviceModels.Where(x => expectedDeviceModel.Name.Equals(x.Name, StringComparison.Ordinal))
                });

            var popoverProvider = RenderComponent<MudPopoverProvider>();

            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<DeviceModelDto>>();

            // Act
            autocompleteComponent.Find("input").Input(expectedDeviceModel.Name);

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-popover-open").Count.Should().Be(1));
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));

            var items = popoverProvider.FindComponents<MudListItem>().ToArray();
            _ = items.Length.Should().Be(1);
            _ = items.First().Markup.Should().Contain(expectedDeviceModel.Name);
            items.First().Find("div.mud-list-item").Click();

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ImportDevices_FileAdded_DevicesExported()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            List<InputFileContent> Files = new()
            {
                InputFileContent.CreateFromText(Guid.NewGuid().ToString(), "upload.csv", contentType: "text/csv")
            };

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true } }
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<ImportReportDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("loading..."));

            cut.Find("#manageDevicesButtonToggle").Click();
            var multiple = popoverProvider.FindComponent<MudFileUpload<IBrowserFile>>();
            var multipleInput = multiple.FindComponent<InputFile>();

            // Act
            multipleInput.UploadFiles(Files.ToArray());

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void Sort_ClickOnSortIdAsc_DevicesSorted()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=Id asc&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceListPage>();

            // Act
            cut.WaitForElement("#sortDeviceId").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void Sort_ClickOnSortIdDesc_DevicesSorted()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=Id asc&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=10&searchText=&searchStatus=&searchState=&orderBy=Id desc&modelId="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceListPage>();

            // Act
            cut.WaitForElement("#sortDeviceId").Click();
            cut.WaitForElement("#sortDeviceId").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}

namespace IoTHub.Portal.Client.Pages.Devices
{
    using System.Collections.Generic;
    using System.Net.Http.Headers;
    using System.Web;
    using Dialogs.Devices;
    using Exceptions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.JSInterop;
    using MudBlazor;
    using Portal.Models.v10;
    using Portal.Shared.Models.v10;
    using Shared;

    public partial class DeviceListPage
    {
        public MudDataGrid<DeviceListItem> devicesGrid = new();

        private bool menuIsOpen;
        private readonly string searchString = string.Empty;
        private readonly string? searchStatus = string.Empty;
        private readonly string? searchState = string.Empty;

        private readonly Dictionary<string, string> searchTags = new();

        private List<DeviceModelDto> deviceModels = new();
        private readonly List<DeviceModelDto> selectedDeviceModels = new();

        private bool IsLoading { get; set; } = true;
        private List<DeviceTagDto> TagList { get; set; } = new();
        private List<LabelDto> labels = new();
        private readonly List<LabelDto> selectedLabels = new();
        private readonly int[] pageSizeOptions = { 10, 20, 50, 100, int.MaxValue };

        [CascadingParameter] public Error? Error { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Gets the custom tags that can be searched via the panel
                TagList = (await DeviceTagSettingsClientService.GetDeviceTags()).ToList();
                foreach (var tag in TagList.Where(tag => tag.Searchable))
                    this.searchTags.Add(tag.Name, "");

                this.labels = (await DeviceClientService.GetAvailableLabels()).ToList();
            }
            catch (ProblemDetailsException exception)
            {
                Error?.ProcessProblemDetails(exception);
            }
        }

        private async Task<GridData<DeviceListItem>> LoadItems(GridState<DeviceListItem> state)
        {
            try
            {
                var sortByDefinition = state.SortDefinitions.FirstOrDefault();
                var columnName = this.devicesGrid.RenderedColumns
                    .Find(x => x.PropertyName == sortByDefinition?.SortBy)
                    ?.Title;
                var propertyName = columnName switch
                {
                    "Device" => nameof(DeviceListItem.Name),
                    "Allowed" => nameof(DeviceListItem.IsEnabled),
                    "Connection state" => nameof(DeviceListItem.IsConnected),
                    _ => nameof(DeviceListItem.Name)
                };

                var orderBy = sortByDefinition == null
                    ? "Name asc"
                    : sortByDefinition.Descending switch
                    {
                        false => // ascending
                            $"{propertyName} asc",
                        true => // descending
                            $"{propertyName} desc",
                    };

                var uri = $"api/devices?pageNumber={state.Page}" +
                          $"&pageSize={state.PageSize}" +
                          $"&searchText={HttpUtility.UrlEncode(this.searchString)}" +
                          $"&searchStatus={this.searchStatus}" +
                          $"&searchState={this.searchState}" +
                          $"&orderBy={orderBy}";

                uri = this.selectedDeviceModels.Aggregate(uri,
                    (current, model) => current + $"&modelId={model.ModelId}");

                uri = this.searchTags.Where(c => !string.IsNullOrEmpty(c.Value)).Aggregate(uri,
                    (current, searchTag) => current + $"&tag.{searchTag.Key}={searchTag.Value}");

                uri = this.selectedLabels.Aggregate(uri,
                    (current, label) => current + $"&labels={label.Name}");

                var result = await DeviceClientService.GetDevices(uri);
                this.deviceModels = await GetDeviceModels();

                return new GridData<DeviceListItem>
                {
                    Items = result.Items,
                    TotalItems = result.TotalItems
                };
            }
            catch (ProblemDetailsException exception)
            {
                Error?.ProcessProblemDetails(exception);

                return new GridData<DeviceListItem>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddDevice()
        {
            NavigationManager.NavigateTo("devices/new");
        }

        private void Refresh()
        {
            _ = this.devicesGrid.ReloadServerData();
        }

        /// <summary>
        /// Prompts a pop-up windows to confirm the device's deletion.
        /// </summary>
        /// <param name="device">Device to delete from the hub</param>
        /// <returns></returns>
        private async Task DeleteDevice(DeviceListItem device)
        {
            var parameters = new DialogParameters
            {
                { "deviceID", device.DeviceID },
                { "deviceName", device.Name },
                { "IsLoRaWan", device.SupportLoRaFeatures }
            };

            _ = await DialogService.Show<DeleteDevicePage>("Confirm Deletion", parameters).Result;
        }

        private void GoToDetails(DeviceListItem item)
        {
            NavigationManager.NavigateTo(
                $"devices/{item.DeviceID}{((item.SupportLoRaFeatures && Portal.IsLoRaSupported) ? "?isLora=true" : "")}");
        }

        /// <summary>
        /// Allows to autocomplete the Device Model field in the form.
        /// </summary>
        /// <param name="value">Text entered in the field</param>
        /// <returns>Item of the device model list that matches the user's value</returns>
        public async Task Search()
        {
            _ = this.devicesGrid.ReloadServerData();
            //var filter = new DeviceModelFilter
            //{
            //    SearchText = value,
            //    PageNumber = 0,
            //    PageSize = 10,
            //    OrderBy = new[] { string.Empty }
            //};
            //return (await DeviceModelsClientService.GetDeviceModelsAsync(filter)).Items.ToList();
        }

        private async Task ExportDeviceList()
        {
            this.menuIsOpen = false;

            var export = await DeviceClientService.ExportDeviceList();
            var fileName = export.Headers.ContentDisposition?.Parameters.Single(c => c.Name == "filename").Value;

            using var streamRef = new DotNetStreamReference(stream: await (export?.ReadAsStreamAsync())!);
            await Js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }

        private async Task ExportTemplateFile()
        {
            this.menuIsOpen = false;

            var export = await DeviceClientService.ExportTemplateFile();
            var fileName = export.Headers.ContentDisposition?.Parameters.Single(c => c.Name == "filename").Value;

            using var streamRef = new DotNetStreamReference(stream: await (export?.ReadAsStreamAsync())!);
            await Js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }

        private async Task ShowDeviceTelemetry(DeviceListItem device)
        {
            var parameters = new DialogParameters { { "DeviceID", device.DeviceID } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };

            _ = await (await DialogService.ShowAsync<LoRaDeviceTelemetryDialog>(string.Empty, parameters, options)).Result;
        }

        private async Task ImportDeviceList(IBrowserFile? file)
        {
            this.menuIsOpen = false;

            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            var content = new MultipartFormDataContent();
            content.Add(content: fileContent,
                name: "\"file\"",
                fileName: file.Name);

            var parameters = new DialogParameters { { "Content", content } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
            _ = await DialogService.Show<ImportReportDialog>("Import summary", parameters, options).Result;
        }

        private void ToggleOpen()
        {
            this.menuIsOpen = !this.menuIsOpen;
        }

        private async Task<List<DeviceModelDto>> GetDeviceModels()
        {
            return (await DeviceModelsClientService.GetDeviceModelsAsync()).Items.ToList();
        }
    }
}

//// Copyright (c) CGI France. All rights reserved.
//// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//namespace IoTHub.Portal.Client.Components.Commons
//{
//    using Enums;
//    using MudBlazor;

//    public partial class DeviceModelDataGridFilter
//    {
//        private bool filterOpen = false;
//        private bool selectAll = true;
//        private HashSet<DeviceModelDto> selectedItems = new();
//        private HashSet<DeviceModelDto> filterItems = new();
//        private FilterDefinition<DeviceModelDto> filterDefinition;


//        [Parameter] public IEnumerable<DeviceModelDto> Items { get; set; }

//        //[Parameter] public FilterContext<DeviceModelDto> Context {get;}

//        protected override Task OnInitializedAsync()
//        {
//            this.selectedItems = Items.ToHashSet();
//            this.filterItems = Items.ToHashSet();
//            this.filterDefinition = new FilterDefinition<DeviceModelDto>
//            {
//                FilterFunction = x => this.filterItems.Contains(x)
//            };

//            return base.OnInitializedAsync();
//        }

//        //private void OpenFilter()
//        //{
//        //    this.filterOpen = true;
//        //}

//        private void SelectedChanged(bool value, DeviceModelDto item)
//        {
//            _ = value ? this.selectedItems.Add(item) : this.selectedItems.Remove(item);

//            this.selectAll = this.selectedItems.Count == Items.Count();
//        }

//        //private async Task ApplyFilterAsync(FilterContext<DeviceModelDto> context)
//        //{
//        //    filterItems = selectedItems.ToHashSet();
//        //    //_icon = _filterItems.Count == Items.Count() ? Icons.Material.Outlined.FilterAlt : Icons.Material.Filled.FilterAlt;
//        //    await context.Actions.ApplyFilterAsync(filterDefinition);
//        //    filterOpen = false;
//        //}

//        private void SelectAll(bool value)
//        {
//            selectAll = value;

//            if (value)
//            {
//                selectedItems = Items.ToHashSet();
//            }
//            else
//            {
//                selectedItems.Clear();
//            }
//        }
//    }
//}

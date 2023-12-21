// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Security
{
    /// <summary>
    /// Policies names.
    /// </summary>
    public static class Policies
    {
        //Group Management :
        public const string GetAllGroups = nameof(GetAllGroups);
        public const string GetGroupDetails = nameof(GetGroupDetails);
        public const string CreateGroup = nameof(CreateGroup);
        public const string UpdateGroup = nameof(UpdateGroup);
        public const string DeleteGroup = nameof(DeleteGroup);
        public const string GetGroupAvatar = nameof(GetGroupAvatar);
        public const string UpdateGroupAvatar = nameof(UpdateGroupAvatar);
        public const string DeleteGroupAvatar = nameof(DeleteGroupAvatar);

        //Group Member Management :
        public const string GetGroupMembers = nameof(GetGroupMembers);
        public const string AddGroupMembers = nameof(AddGroupMembers);
        public const string RemoveGroupMembers = nameof(RemoveGroupMembers);

        // Role Management :
        public const string GetAllRoles = nameof(GetAllRoles);
        public const string GetRoleDetails = nameof(GetRoleDetails);
        public const string CreateRole = nameof(CreateRole);
        public const string UpdateRole = nameof(UpdateRole);
        public const string DeleteRole = nameof(DeleteRole);
        public const string GetRoleAvatar = nameof(GetRoleAvatar);
        public const string UpdateRoleAvatar = nameof(UpdateRoleAvatar);
        public const string DeleteRoleAvatar = nameof(DeleteRoleAvatar);

        // Access Control Management :
        public const string GetAccessControls = nameof(GetAccessControls);
        public const string AddAccessControl = nameof(AddAccessControl);
        public const string RemoveAccessControl = nameof(RemoveAccessControl);
        public const string EditAccessControl = nameof(EditAccessControl);
        public const string GetHierarchicalScopes = nameof(GetHierarchicalScopes);
        public const string CreateHierarchicalScope = nameof(CreateHierarchicalScope);
        public const string UpdateHierarchicalScope = nameof(UpdateHierarchicalScope);
        public const string DeleteHierarchicalScope = nameof(DeleteHierarchicalScope);

        // Admin APIs :
        public const string ExportDevices = nameof(ExportDevices);
        public const string ImportDevices = nameof(ImportDevices);
        public const string DownloadDeviceTemplate = nameof(DownloadDeviceTemplate);

        // Metrics : 
        public const string GetPortalMetrics = nameof(GetPortalMetrics);

        //IoT Device :
        public const string GetAllDeviceConfigurations = nameof(GetAllDeviceConfigurations);
        public const string CreateDeviceConfiguration = nameof(CreateDeviceConfiguration);
        public const string GetDeviceConfiguration = nameof(GetDeviceConfiguration);
        public const string UpdateDeviceConfiguration = nameof(UpdateDeviceConfiguration);
        public const string DeleteDeviceConfiguration = nameof(DeleteDeviceConfiguration);
        public const string GetAllDeviceConfigurationMetrics = nameof(GetAllDeviceConfigurationMetrics);
        public const string GetAllDevices = nameof(GetAllDevices);
        public const string CreateDevice = nameof(CreateDevice);
        public const string UpdateDevice = nameof(UpdateDevice);
        public const string GetDeviceDetails = nameof(GetDeviceDetails);
        public const string DeleteDevice = nameof(DeleteDevice);
        public const string GetDeviceCredentials = nameof(GetDeviceCredentials);
        public const string GetDeviceProperties = nameof(GetDeviceProperties);
        public const string CreateDeviceProperties = nameof(CreateDeviceProperties);
        public const string GetAllAvailableDeviceLabels = nameof(GetAllAvailableDeviceLabels);

        //IoT Edge Device :
        public const string GetAllEdgeDevices = nameof(GetAllEdgeDevices);
        public const string CreateEdgeDevice = nameof(CreateEdgeDevice);
        public const string GetEdgeDevice = nameof(GetEdgeDevice);
        public const string UpdateEdgeDevice = nameof(UpdateEdgeDevice);
        public const string DeleteEdgeDevice = nameof(DeleteEdgeDevice);
        public const string ExecuteEdgeModuleMethod = nameof(ExecuteEdgeModuleMethod);
        public const string GetEdgeDeviceCredentials = nameof(GetEdgeDeviceCredentials);
        public const string GetEdgeDeviceEnrollmentScriptUrl = nameof(GetEdgeDeviceEnrollmentScriptUrl);
        public const string GetEdgeDeviceLogs = nameof(GetEdgeDeviceLogs);
        public const string GetAllAvailableEdgeDeviceLabels = nameof(GetAllAvailableEdgeDeviceLabels);
        public const string GetEdgeDeviceEnrollmentScript = nameof(GetEdgeDeviceEnrollmentScript);

        // IoT Edge Device Models :
        public const string GetAllEdgeModel = nameof(GetAllEdgeModel);
        public const string CreateEdgeModel = nameof(CreateEdgeModel);
        public const string UpdateEdgeModel = nameof(UpdateEdgeModel);
        public const string GetEdgeModel = nameof(GetEdgeModel);
        public const string DeleteEdgeModel = nameof(DeleteEdgeModel);
        public const string GetEdgeModelAvatar = nameof(GetEdgeModelAvatar);
        public const string UpdateEdgeModelAvatar = nameof(UpdateEdgeModelAvatar);
        public const string DeleteEdgeModelAvatar = nameof(DeleteEdgeModelAvatar);
        public const string GetPublicEdgeModules = nameof(GetPublicEdgeModules);

        // Ideas :
        public const string SumitIdea = nameof(SumitIdea);

        //LoRaWan Device :
        public const string GetAllConcentrators = nameof(GetAllConcentrators);
        public const string CreateConcentrator = nameof(CreateConcentrator);
        public const string UpdateConcentrator = nameof(UpdateConcentrator);
        public const string GetConcentrator = nameof(GetConcentrator);
        public const string DeleteConcentrator = nameof(DeleteConcentrator);
        public const string GetAllLorawanDevices = nameof(GetAllLorawanDevices);
        public const string CreateLorawanDevice = nameof(CreateLorawanDevice);
        public const string UpdateLorawanDevice = nameof(UpdateLorawanDevice);
        public const string GetLorawanDevice = nameof(GetLorawanDevice);
        public const string DeleteLorawanDevice = nameof(DeleteLorawanDevice);
        public const string ExecuteLorawanDeviceCommand = nameof(ExecuteLorawanDeviceCommand);
        public const string GetLorwanDeviceTelemetry = nameof(GetLorwanDeviceTelemetry);
        public const string GetAvailableLorawanDeviceLabels = nameof(GetAvailableLorawanDeviceLabels);
        public const string GetLorawanDeviceGateways = nameof(GetLorawanDeviceGateways);
        public const string GetFrequencyPlans = nameof(GetFrequencyPlans);
        public const string GetAllLorawanDeviceModels = nameof(GetAllLorawanDeviceModels);
        public const string CreateLorawanDeviceModel = nameof(CreateLorawanDeviceModel);
        public const string GetLorawanDeviceModel = nameof(GetLorawanDeviceModel);
        public const string UpdateLorawanDeviceModel = nameof(UpdateLorawanDeviceModel);
        public const string DeleteLorawanDeviceModel = nameof(DeleteLorawanDeviceModel);
        public const string GetLorawanDeviceModelAvatar = nameof(GetLorawanDeviceModelAvatar);
        public const string UpdateLorawanDeviceModelAvatar = nameof(UpdateLorawanDeviceModelAvatar);
        public const string DeleteLorawanDeviceModelAvatar = nameof(DeleteLorawanDeviceModelAvatar);
        public const string UpdateLorawanDeviceModelCommands = nameof(UpdateLorawanDeviceModelCommands);
        public const string GetLorawanDeviceModelCommands = nameof(GetLorawanDeviceModelCommands);

        // Device Models :
        public const string GetAllDeviceModels = nameof(GetAllDeviceModels);
        public const string CreateDeviceModel = nameof(CreateDeviceModel);
        public const string GetDeviceModel = nameof(GetDeviceModel);
        public const string UpdateDeviceModel = nameof(UpdateDeviceModel);
        public const string DeleteDeviceModel = nameof(DeleteDeviceModel);
        public const string GetDeviceModelAvatar = nameof(GetDeviceModelAvatar);
        public const string UpdateDeviceModelAvatar = nameof(UpdateDeviceModelAvatar);
        public const string DeleteDeviceModelAvatar = nameof(DeleteDeviceModelAvatar);
        public const string GetDeviceModelProperties = nameof(GetDeviceModelProperties);
        public const string SetDeviceModelProperties = nameof(SetDeviceModelProperties);

        // Portal Settings :
        public const string UpdateDeviceTagSettings = nameof(UpdateDeviceTagSettings);
        public const string GetAllDeviceTagSettings = nameof(GetAllDeviceTagSettings);
        public const string CreateOrUpdateDeviceTag = nameof(CreateOrUpdateDeviceTag);
        public const string DeleteDeviceTagByName = nameof(DeleteDeviceTagByName);
        public const string GetOIDCSettings = nameof(GetOIDCSettings);
        public const string GetPortalSettings = nameof(GetPortalSettings);
    }
}

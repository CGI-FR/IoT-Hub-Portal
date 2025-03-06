// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Security
{
    /// <summary>
    /// Policies names.
    /// </summary>
    public enum Policies
    {
        // Group Management
        GetAllGroups,
        GetGroupDetails,
        CreateGroup,
        UpdateGroup,
        DeleteGroup,
        GetGroupAvatar,
        UpdateGroupAvatar,
        DeleteGroupAvatar,

        // Group Member Management
        GetGroupMembers,
        AddGroupMembers,
        RemoveGroupMembers,

        // Role Management
        GetAllRoles,
        GetRoleDetails,
        CreateRole,
        UpdateRole,
        DeleteRole,
        GetRoleAvatar,
        UpdateRoleAvatar,
        DeleteRoleAvatar,

        // Access Control Management
        GetAccessControls,
        AddAccessControl,
        RemoveAccessControl,
        EditAccessControl,
        GetHierarchicalScopes,
        CreateHierarchicalScope,
        UpdateHierarchicalScope,
        DeleteHierarchicalScope,

        // Admin APIs
        ExportDevices,
        ImportDevices,
        DownloadDeviceTemplate,

        // Metrics
        GetPortalMetrics,

        // IoT Device
        GetAllDeviceConfigurations,
        CreateDeviceConfiguration,
        GetDeviceConfiguration,
        UpdateDeviceConfiguration,
        DeleteDeviceConfiguration,
        GetAllDeviceConfigurationMetrics,
        GetAllDevices,
        CreateDevice,
        UpdateDevice,
        GetDeviceDetails,
        DeleteDevice,
        GetDeviceCredentials,
        GetDeviceProperties,
        CreateDeviceProperties,
        GetAllAvailableDeviceLabels,

        // IoT Edge Device
        GetAllEdgeDevices,
        CreateEdgeDevice,
        GetEdgeDevice,
        UpdateEdgeDevice,
        DeleteEdgeDevice,
        ExecuteEdgeModuleMethod,
        GetEdgeDeviceCredentials,
        GetEdgeDeviceEnrollmentScriptUrl,
        GetEdgeDeviceLogs,
        GetAllAvailableEdgeDeviceLabels,
        GetEdgeDeviceEnrollmentScript,

        // IoT Edge Device Models
        GetAllEdgeModel,
        CreateEdgeModel,
        UpdateEdgeModel,
        GetEdgeModel,
        DeleteEdgeModel,
        GetEdgeModelAvatar,
        UpdateEdgeModelAvatar,
        DeleteEdgeModelAvatar,
        GetPublicEdgeModules,

        // Ideas
        SumitIdea,

        // LoRaWan Device
        GetAllConcentrators,
        CreateConcentrator,
        UpdateConcentrator,
        GetConcentrator,
        DeleteConcentrator,
        GetAllLorawanDevices,
        CreateLorawanDevice,
        UpdateLorawanDevice,
        GetLorawanDevice,
        DeleteLorawanDevice,
        ExecuteLorawanDeviceCommand,
        GetLorwanDeviceTelemetry,
        GetAvailableLorawanDeviceLabels,
        GetLorawanDeviceGateways,
        GetFrequencyPlans,
        GetAllLorawanDeviceModels,
        CreateLorawanDeviceModel,
        GetLorawanDeviceModel,
        UpdateLorawanDeviceModel,
        DeleteLorawanDeviceModel,
        GetLorawanDeviceModelAvatar,
        UpdateLorawanDeviceModelAvatar,
        DeleteLorawanDeviceModelAvatar,
        UpdateLorawanDeviceModelCommands,
        GetLorawanDeviceModelCommands,

        // Device Models
        GetAllDeviceModels,
        CreateDeviceModel,
        GetDeviceModel,
        UpdateDeviceModel,
        DeleteDeviceModel,
        GetDeviceModelAvatar,
        UpdateDeviceModelAvatar,
        DeleteDeviceModelAvatar,
        GetDeviceModelProperties,
        SetDeviceModelProperties,

        // Portal Settings
        UpdateDeviceTagSettings,
        GetAllDeviceTagSettings,
        CreateOrUpdateDeviceTag,
        DeleteDeviceTagByName,
        GetOIDCSettings,
        GetPortalSettings
    }
}


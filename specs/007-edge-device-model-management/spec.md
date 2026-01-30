# Feature Specification: Edge Device Model Management

**Feature ID**: 007  
**Feature Branch**: `007-edge-device-model-management`  
**Created**: 2025-01-21  
**Status**: Draft  
**Source**: Analysis from `specs/007-edge-device-model-management/analyze.md`

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Deploy Edge Device Model (Priority: P1)

As an IoT administrator, I need to create standardized edge device model templates that define the complete configuration for edge devices, including modules, routes, and settings, so that I can deploy consistent configurations across multiple edge devices without manual setup.

**Why this priority**: This is the foundational capability that enables infrastructure-as-code for edge deployments. Without the ability to create models, no other functionality is possible. This directly addresses the critical business need to reduce manual configuration errors and ensure consistency across device fleets.

**Independent Test**: Can be fully tested by creating a new edge model with modules and routes, saving it, and verifying the configuration is stored and deployed to the cloud provider. Delivers immediate value by establishing a reusable template.

**Acceptance Scenarios**:

1. **Given** I am an authorized administrator with write permissions, **When** I navigate to create a new edge model and provide a unique name, description, and at least one custom module with a container image, **Then** the system creates the model, generates a unique identifier, deploys the configuration to the cloud provider, and displays a success confirmation.

2. **Given** I am creating an Azure IoT Edge model, **When** I configure system modules (edgeAgent, edgeHub) with custom runtime images and define message routes between modules, **Then** the system validates the route syntax and includes both system modules and routes in the deployed configuration.

3. **Given** I am creating an AWS IoT Greengrass model, **When** I configure custom components without system modules, **Then** the system creates a Greengrass deployment configuration with the specified components and their versions.

4. **Given** I attempt to create a model with a name that already exists, **When** I submit the form, **Then** the system prevents creation and displays an error message indicating the model already exists.

5. **Given** I am configuring a module, **When** I add environment variables and module twin settings, **Then** the system stores these configurations and includes them in the deployment so they are injected into the container at runtime.

---

### User Story 2 - Browse and Search Edge Models (Priority: P1)

As an IoT administrator, I need to view a searchable list of all edge device models with visual identification and categorization, so that I can quickly find and select the appropriate model for device provisioning.

**Why this priority**: This is essential for model discoverability and selection. Administrators must be able to locate existing models before they can be assigned to devices. Without this capability, model reuse becomes impractical.

**Independent Test**: Can be fully tested by navigating to the edge model list, searching by keywords, and verifying that matching models are displayed with their names, descriptions, avatars, and labels. Delivers value by enabling efficient model discovery.

**Acceptance Scenarios**:

1. **Given** multiple edge models exist in the system, **When** I navigate to the edge models list page, **Then** the system displays a paginated table showing model name, description, avatar image, and labels for each model.

2. **Given** I am viewing the edge models list, **When** I enter a search keyword in the filter field, **Then** the system displays only models whose name or description contains the keyword (case-insensitive).

3. **Given** models have assigned labels for categorization, **When** I view the model list, **Then** each model displays its associated labels for quick visual categorization.

4. **Given** I am viewing the model list, **When** I click on a model row, **Then** the system navigates to the detailed model view showing complete configuration.

5. **Given** I have read-only permissions, **When** I view the model list, **Then** I can browse and search models but action buttons for create and delete are hidden.

---

### User Story 3 - View and Update Edge Model Configuration (Priority: P1)

As an IoT administrator, I need to view detailed edge model configurations and update them when requirements change, so that deployed devices receive updated configurations without manual reconfiguration.

**Why this priority**: Model configurations evolve over time. The ability to update models is critical for maintaining and improving deployments. This enables continuous improvement of edge device behavior.

**Independent Test**: Can be fully tested by opening an existing model, modifying module settings or routes, saving the changes, and verifying the updated configuration is deployed. Delivers value by allowing model evolution.

**Acceptance Scenarios**:

1. **Given** I am viewing an edge model detail page, **When** the page loads, **Then** the system displays all model properties including general details, modules (system and custom), routes, environment variables, twin settings, and labels in an organized tabbed interface.

2. **Given** I have write permissions and modify module environment variables, **When** I save the model, **Then** the system updates the database, redeploys the configuration to the cloud provider, and confirms successful update.

3. **Given** I am updating an Azure model's message routes, **When** I modify route definitions with valid FROM/INTO syntax, **Then** the system validates the route format and updates the routing configuration in IoT Hub.

4. **Given** I add a new custom module to an existing model, **When** I provide the module name and container image URI, **Then** the system includes the new module in the model configuration and deploys it.

5. **Given** I update module commands for an Azure model, **When** I save changes, **Then** the system synchronizes the commands to the database for quick command lookup without cloud API calls.

6. **Given** I attempt to update a model that no longer exists, **When** I submit changes, **Then** the system displays a "not found" error and prevents the update.

---

### User Story 4 - Manage Model Visual Identity (Priority: P2)

As an IoT administrator, I need to assign custom avatar images to edge models, so that I can quickly visually identify different model types in lists and dashboards.

**Why this priority**: Visual identification significantly improves user experience and reduces cognitive load when working with multiple models. While not critical for core functionality, it substantially improves operational efficiency.

**Independent Test**: Can be fully tested by uploading an avatar image to a model, verifying it displays in the list view, and testing the delete functionality to revert to default. Delivers value through improved visual navigation.

**Acceptance Scenarios**:

1. **Given** I am viewing an edge model detail page, **When** I upload a custom avatar image (JPG, JPEG, or PNG), **Then** the system stores the image and displays it in place of the default placeholder in both detail and list views.

2. **Given** a model has a custom avatar, **When** I delete the avatar, **Then** the system removes the custom image and reverts to displaying the default placeholder image.

3. **Given** I am viewing the edge models list, **When** the page loads, **Then** each model displays its avatar image for quick visual identification.

4. **Given** I have read-only permissions, **When** I view a model, **Then** I can see the avatar but cannot upload or delete it.

---

### User Story 5 - Delete Obsolete Edge Models (Priority: P2)

As an IoT administrator, I need to delete edge device models that are no longer needed, so that the model catalog remains clean and relevant.

**Why this priority**: Model lifecycle management is important for maintaining a clean system, but deletion is a less frequent operation compared to creation and updates. It's essential for long-term system hygiene but not for initial deployment.

**Independent Test**: Can be fully tested by selecting a model for deletion, confirming the action, and verifying the model is removed from the database and cloud provider. Delivers value by preventing catalog bloat.

**Acceptance Scenarios**:

1. **Given** I have write permissions, **When** I click the delete button for an edge model and confirm the deletion in the dialog, **Then** the system removes the model from the database, deletes the cloud configuration, removes associated labels and commands, and displays a success message.

2. **Given** I initiate model deletion, **When** the confirmation dialog appears, **Then** the dialog displays the model name and ID for verification before proceeding.

3. **Given** a model has associated module commands and labels, **When** I delete the model, **Then** the system cascades the deletion to all related records including commands, labels, and avatar images.

4. **Given** I cancel the deletion confirmation dialog, **When** I click cancel, **Then** the system closes the dialog without deleting the model.

5. **Given** devices are currently using a model, **When** I attempt to delete it, **Then** [NEEDS CLARIFICATION: Should deletion be blocked or should it proceed with devices retaining their current configuration?]

---

### User Story 6 - Leverage Public Module Catalog (Priority: P3)

As an IoT administrator, I need to import pre-configured modules from a public catalog when creating edge models, so that I can accelerate deployment with tested components and reduce configuration errors.

**Why this priority**: While valuable for accelerating deployments, this is an enhancement that builds on top of the core model creation capability. Organizations can still create models manually without catalog integration.

**Independent Test**: Can be fully tested by accessing the public modules catalog during model creation, selecting a module, and verifying it's imported with all its configuration. Delivers value through time savings and reduced errors.

**Acceptance Scenarios**:

1. **Given** I am creating a new edge model, **When** I access the public modules catalog, **Then** the system displays available pre-configured modules with their names, descriptions, and container images.

2. **Given** I select a module from the public catalog, **When** I import it, **Then** the system adds the module to my model configuration with all pre-configured settings including environment variables and twin settings.

3. **Given** the public catalog is unavailable or empty, **When** I attempt to access it, **Then** the system displays an appropriate message indicating no modules are available.

---

### User Story 7 - Categorize Models with Labels (Priority: P3)

As an IoT administrator, I need to assign labels to edge models for categorization purposes, so that I can organize models by environment, version, purpose, or any other classification scheme.

**Why this priority**: Labels improve organization and filtering but are not required for core model functionality. This is a valuable enhancement for larger deployments with many models.

**Independent Test**: Can be fully tested by assigning labels to a model, saving it, and verifying the labels display in the list view. Delivers value through improved organization.

**Acceptance Scenarios**:

1. **Given** I am creating or editing an edge model, **When** I assign labels such as "production", "testing", or "v2.0", **Then** the system stores the label associations and displays them with the model.

2. **Given** a model has assigned labels, **When** I view the model in the list, **Then** the labels are displayed alongside the model name for quick categorization.

3. **Given** I am updating a model's labels, **When** I remove existing labels and add new ones, **Then** the system replaces the old labels with the new ones (delete-all, insert-all operation).

4. **Given** I delete a model, **When** the deletion completes, **Then** the system cascades the deletion to remove all label associations.

---

### User Story 8 - Configure Azure-Specific Features (Priority: P2)

As an IoT administrator working with Azure IoT Edge, I need to configure system modules (edgeAgent, edgeHub), message routes, and module commands, so that I can leverage Azure's full edge runtime capabilities.

**Why this priority**: Essential for Azure deployments but not applicable to AWS users. This is a high priority for Azure-based organizations but does not block AWS functionality.

**Independent Test**: Can be fully tested by creating an Azure model, configuring system modules with custom runtime images, defining routes between modules, and adding module commands. Delivers value through Azure-specific edge orchestration.

**Acceptance Scenarios**:

1. **Given** I am creating an Azure IoT Edge model, **When** the model initializes, **Then** the system automatically includes edgeAgent and edgeHub system modules with default configurations.

2. **Given** I am configuring system modules, **When** I specify custom runtime images for edgeAgent or edgeHub, **Then** the system uses these images in the deployed configuration.

3. **Given** I am defining message routes, **When** I create a route with the syntax "FROM /messages/modules/tempSensor/outputs/temperatureOutput INTO $upstream", **Then** the system validates the syntax and includes the route in the IoT Hub configuration.

4. **Given** I am adding module commands, **When** I define a command name and associate it with a module, **Then** the system stores the command in the database for quick lookup during remote command execution.

5. **Given** I am configuring a route with optional properties, **When** I set route priority (0-9) and time-to-live in seconds, **Then** the system validates the ranges and includes these properties in the route configuration.

---

### User Story 9 - Configure AWS Greengrass Deployments (Priority: P2)

As an IoT administrator working with AWS IoT Greengrass, I need to define component-based edge models without system modules or routes, so that I can deploy Greengrass configurations that align with AWS's architecture.

**Why this priority**: Essential for AWS deployments but not applicable to Azure users. This is a high priority for AWS-based organizations but does not block Azure functionality.

**Independent Test**: Can be fully tested by creating an AWS Greengrass model, configuring custom components with versions, and verifying the deployment is created in AWS IoT Core. Delivers value through AWS-specific edge deployment.

**Acceptance Scenarios**:

1. **Given** I am creating an AWS IoT Greengrass model, **When** I configure the model, **Then** the system does not include system modules or routes (AWS uses a different architecture).

2. **Given** I am defining Greengrass components, **When** I specify component names and versions, **Then** the system creates a Greengrass deployment configuration targeting the appropriate thing group.

3. **Given** I am configuring a Greengrass component, **When** I set component-specific settings, **Then** the system structures the configuration according to Greengrass component recipe format.

4. **Given** I save an AWS model, **When** the deployment is created, **Then** the system stores the AWS deployment ID as the external identifier for tracking.

---

### Edge Cases

- **What happens when a model is created but cloud deployment fails?** System should rollback the database record or mark the model as "deployment failed" to prevent orphaned records.

- **What happens when updating a model that is currently assigned to active devices?** [NEEDS CLARIFICATION: Are devices automatically redeployed with the new configuration, or does update only affect future device provisioning?]

- **How does the system handle module container image URIs that are inaccessible or invalid?** System should validate image URI format but may not be able to verify accessibility until deployment. Cloud provider will report deployment failures.

- **What happens when a model is deleted but the cloud configuration deletion fails?** System should either rollback the database deletion or mark the model for retry to prevent inconsistent state.

- **What happens when a user attempts to create a route with invalid syntax?** The regex validation should catch syntax errors and display inline error messages before submission.

- **How does the system handle concurrent updates to the same model by multiple administrators?** Last-write-wins pattern with database-level concurrency control. Consider adding optimistic concurrency checks.

- **What happens when label associations fail during model update?** Label updates are transactional within unit of work. Failure should rollback all changes.

- **How are container registry credentials managed for private images?** [NEEDS CLARIFICATION: Current implementation doesn't expose credential management in UI. How are credentials configured at cloud provider level?]

- **What happens when importing a public module that conflicts with existing module names?** System should allow the import and let the user resolve naming conflicts manually.

- **How does the system handle very large numbers of modules or routes in a single model?** UI should handle pagination or virtual scrolling for large collections. Consider performance testing with 50+ modules.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow authorized administrators to create new edge device models with a unique name, description, and at least one custom module configuration.

- **FR-002**: System MUST generate a unique model identifier (GUID) for each new edge model automatically.

- **FR-003**: System MUST deploy edge model configurations to the configured cloud provider (Azure IoT Hub or AWS IoT Core) when models are created or updated.

- **FR-004**: System MUST store an external identifier linking the local database record to the cloud provider's configuration or deployment ID.

- **FR-005**: System MUST support filtering edge models by keyword search across model name and description fields (case-insensitive).

- **FR-006**: System MUST display edge model avatars in list and detail views for visual identification.

- **FR-007**: System MUST allow administrators to upload custom avatar images (JPG, JPEG, PNG) and delete them to revert to default placeholder.

- **FR-008**: System MUST validate that edge model names are unique before creation.

- **FR-009**: System MUST prevent creation of models with duplicate identifiers by throwing ResourceAlreadyExistsException.

- **FR-010**: System MUST allow updating of existing edge models including modules, routes, labels, and configuration settings.

- **FR-011**: System MUST cascade update operations to the cloud provider immediately upon save.

- **FR-012**: System MUST allow deletion of edge models with confirmation, including removal of cloud configurations and cascade deletion of associated records.

- **FR-013**: System MUST support assignment of multiple labels to edge models for categorization and organization.

- **FR-014**: System MUST handle label updates by replacing all existing labels with the new set (delete-all, insert-all operation).

- **FR-015**: System MUST enforce permission-based access control with "edge-model:read" for viewing and "edge-model:write" for modifications.

- **FR-016**: System MUST display read-only views to users without write permissions, hiding action buttons and disabling form fields.

- **FR-017**: System MUST provide a public modules catalog accessible during model creation for importing pre-configured modules.

- **FR-018**: System MUST support configuration of custom edge modules with the following properties: module name, container image URI, container create options, startup order, status, and version.

- **FR-019**: System MUST allow defining environment variables as key-value pairs for each module to be injected at container runtime.

- **FR-020**: System MUST allow defining module twin settings (desired properties) for each module to be synchronized via cloud provider.

- **FR-021**: System MUST validate module names are required and module image URIs are required for all custom modules.

- **FR-022**: System MUST provide paginated list views for edge models with configurable page sizes.

- **FR-023**: System MUST organize model detail and creation pages with tabbed interfaces for General, Modules, and Routes sections.

- **FR-024**: System MUST display validation errors inline on form fields with user-friendly error messages.

- **FR-025**: System MUST provide visual feedback during asynchronous operations with loading indicators and success/error notifications.

### Azure IoT Edge-Specific Requirements

- **FR-026**: System MUST automatically include edgeAgent and edgeHub system modules when creating Azure IoT Edge models.

- **FR-027**: System MUST allow configuration of system module runtime images for edgeAgent and edgeHub.

- **FR-028**: System MUST support defining message routes with syntax: "FROM <source> [WHERE <condition>] INTO <sink>".

- **FR-029**: System MUST validate route syntax using regex pattern: `^(?i)FROM [\S]+( WHERE (NOT )?[\S]+)? INTO [\S]+$`.

- **FR-030**: System MUST support optional route properties: priority (0-9 range) and time-to-live (non-negative integer seconds).

- **FR-031**: System MUST support module commands associated with specific modules for remote execution capabilities.

- **FR-032**: System MUST store module commands in the database for quick lookup without cloud API calls.

- **FR-033**: System MUST synchronize module commands during model create and update operations.

- **FR-034**: System MUST create Azure IoT Hub automatic device configurations with module deployments and routes.

### AWS IoT Greengrass-Specific Requirements

- **FR-035**: System MUST support AWS IoT Greengrass models without system modules (component-based architecture).

- **FR-036**: System MUST support AWS IoT Greengrass models without message routes (pub/sub communication model).

- **FR-037**: System MUST create AWS IoT Greengrass deployment configurations targeting thing groups.

- **FR-038**: System MUST store AWS deployment IDs as external identifiers for tracking.

### Data Validation Requirements

- **FR-039**: System MUST validate that model names are provided and not empty during creation and update.

- **FR-040**: System MUST validate that module names are provided and not empty for all custom modules.

- **FR-041**: System MUST validate that module image URIs are provided and not empty for all custom modules.

- **FR-042**: System MUST validate route format matches the required syntax before saving.

- **FR-043**: System MUST validate route priority is within 0-9 range when provided.

- **FR-044**: System MUST validate route time-to-live is a non-negative integer when provided.

- **FR-045**: System MUST validate uploaded avatar images are of supported file types (JPG, JPEG, PNG).

### Error Handling Requirements

- **FR-046**: System MUST throw ResourceAlreadyExistsException when attempting to create a model with an existing identifier.

- **FR-047**: System MUST throw ResourceNotFoundException when attempting to retrieve, update, or delete a non-existent model.

- **FR-048**: System MUST display user-friendly error messages via Snackbar notifications for API failures.

- **FR-049**: System MUST maintain transactional consistency using Unit of Work pattern for database operations.

- **FR-050**: System MUST rollback database changes if cloud provider deployment fails [NEEDS CLARIFICATION: Current implementation behavior].

### Key Entities *(include if feature involves data)*

- **EdgeDeviceModel**: Represents an edge device model template with properties including unique identifier, name, description, external identifier (cloud deployment/configuration ID), and associated labels. Serves as the primary entity for edge model management.

- **EdgeDeviceModelCommand**: Represents module commands associated with an edge model (Azure only). Contains command name, associated module name, and links to the parent EdgeDeviceModel. Used for remote command execution capabilities.

- **Label**: Represents categorization tags with many-to-many relationship to EdgeDeviceModel. Used for organizing and filtering models by environment, version, purpose, or custom classifications.

- **EdgeDevice**: References EdgeDeviceModel to determine deployment configuration. Edge devices use the model as a template for their module and route configuration.

- **IoTEdgeModel (DTO)**: Complete data transfer object containing model details, custom modules list, routes list, system modules (Azure), and labels. Used for API communication between client and server.

- **IoTEdgeModule (DTO)**: Represents a custom edge module with properties including module name, container image URI, container create options, startup order, status, version, environment variables, twin settings, and commands.

- **IoTEdgeRoute (DTO)**: Represents a message route with properties including route name, route value (FROM/INTO syntax), optional priority (0-9), and optional time-to-live in seconds.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Administrators can create a complete edge device model including modules and routes in under 5 minutes.

- **SC-002**: System reduces manual edge device configuration errors by 80% through standardized model-based deployment.

- **SC-003**: Model configurations deploy to cloud provider (Azure or AWS) within 30 seconds of save operation.

- **SC-004**: 95% of edge model searches return relevant results on first query attempt.

- **SC-005**: Administrators can locate and open an existing model from the list in under 15 seconds.

- **SC-006**: Model avatar images load and display in list views within 2 seconds per page load.

- **SC-007**: System successfully validates and prevents creation of duplicate model names in 100% of cases.

- **SC-008**: Model update operations complete with cloud provider synchronization in under 45 seconds.

- **SC-009**: Public module catalog imports complete in under 10 seconds per module.

- **SC-010**: System maintains 99.9% consistency between database records and cloud provider configurations.

- **SC-011**: Edge model deletion operations complete with full cleanup (database, cloud, labels, commands) in under 30 seconds.

- **SC-012**: Route syntax validation provides immediate feedback (under 1 second) on invalid route definitions.

- **SC-013**: Users with read-only permissions can browse models without encountering authorization errors or broken functionality.

- **SC-014**: System handles concurrent model updates by multiple administrators without data loss or corruption.

- **SC-015**: Administrators can manage fleets of 100+ edge devices using 10-20 standardized models, reducing configuration complexity by 70%.

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/007-edge-device-model-management/analyze.md`
- **Analyzed By**: excavate.specifier
- **Analysis Date**: 2025-01-21

### Code References

#### Controllers
- `src/IoTHub.Portal.Server/Controllers/v1.0/EdgeModelsController.cs` - REST API endpoints for edge model CRUD operations, avatar management, and public catalog access

#### Business Logic
- `src/IoTHub.Portal.Application/Services/IEdgeModelService.cs` - Service interface defining core edge model operations
- `src/IoTHub.Portal.Infrastructure/Services/EdgeModelService.cs` - Service implementation with cloud provider integration, label management, and command synchronization

#### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IEdgeDeviceModelRepository.cs` - Repository interface for EdgeDeviceModel entity
- `src/IoTHub.Portal.Domain/Repositories/IEdgeDeviceModelCommandRepository.cs` - Repository interface for module commands
- `src/IoTHub.Portal.Domain/Entities/EdgeDeviceModel.cs` - Core entity definition with labels relationship
- `src/IoTHub.Portal.Domain/Entities/EdgeDeviceModelCommand.cs` - Module command entity (Azure-specific)

#### UI Components
- `src/IoTHub.Portal.Client/Pages/EdgeModels/EdgeModelListPage.razor` - Model list view with search, filtering, and pagination
- `src/IoTHub.Portal.Client/Pages/EdgeModels/EdgeModelDetailPage.razor` - Detailed model view and editing with tabbed interface
- `src/IoTHub.Portal.Client/Pages/EdgeModels/CreateEdgeModelsPage.razor` - Model creation page with full configuration wizard
- `src/IoTHub.Portal.Client/Components/EdgeModels/EdgeModelSearch.razor` - Search component for filtering
- `src/IoTHub.Portal.Client/Dialogs/EdgeModels/DeleteEdgeModelDialog.razor` - Deletion confirmation dialog

#### DTOs
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModel.cs` - Complete edge model DTO with modules, routes, system modules, and labels
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModelListItem.cs` - List view DTO for model browsing
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModule.cs` - Edge module definition with environment variables and twin settings
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeRoute.cs` - Route definition with validation rules
- `src/IoTHub.Portal.Shared/Models/v1.0/EdgeModelSystemModule.cs` - System module DTO (Azure)
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModuleCommand.cs` - Module command DTO
- `src/IoTHub.Portal.Shared/Models/v1.0/Filters/EdgeModelFilter.cs` - Filter DTO for search and filtering

#### Client Services
- `src/IoTHub.Portal.Client/Services/IEdgeModelClientService.cs` - Client service interface for HTTP API calls
- `src/IoTHub.Portal.Client/Services/EdgeModelClientService.cs` - HTTP client implementation

#### Validators
- `src/IoTHub.Portal.Client/Validators/EdgeModelValidator.cs` - FluentValidation for edge model DTOs

#### Mappers
- `src/IoTHub.Portal.Application/Mappers/EdgeDeviceModelProfile.cs` - AutoMapper profile for entity-DTO mapping
- `src/IoTHub.Portal.Application/Mappers/EdgeDeviceModelCommandProfile.cs` - AutoMapper profile for command mapping

### Dependencies

**This feature depends on:**
- **Label Management** - Edge models use labels for categorization and filtering
- **Role-Based Access Control** - Permissions enforced through RBAC system with edge-model:read and edge-model:write permissions
- **Azure/AWS Integration** - Multi-cloud deployment orchestration via IConfigService
- **Configuration Management** - Application configuration for cloud provider selection
- **Device Model Image Manager** - Avatar/image storage and retrieval

**Features that depend on this:**
- **Edge Device Management** - Edge devices reference edge models for deployment configuration and module setup
- **Device Configurations** - Edge models create automatic device configurations in cloud providers
- **Dashboard** - May display edge model statistics and deployment metrics
- **Device Provisioning** - New edge devices assigned to models during provisioning

### Related Features
- Feature 001: User Management (authorization)
- Feature 002: Authentication & Authorization (permission enforcement)
- Feature 008: Device Management (edge device provisioning with models)
- Feature 016: Configuration Management (cloud provider settings)

---

## Notes

### Multi-Cloud Architecture
The feature implements a strategy pattern for multi-cloud support, abstracting Azure and AWS differences behind the IConfigService interface. Azure models include system modules (edgeAgent, edgeHub) and message routes, while AWS models use a component-based architecture without these concepts.

### Model Lifecycle and Consistency
Edge models are stored in the local database and synchronized to cloud providers (Azure IoT Hub or AWS IoT Core). The external identifier field links the database record to the cloud configuration, maintaining consistency. Updates trigger immediate rollout to the cloud provider.

### Module Commands Design (Azure-Specific)
Module commands are stored in the EdgeDeviceModelCommand entity for quick lookup without cloud API calls. This optimization enables faster command execution by pre-loading available commands during model retrieval.

### Label Management Pattern
Labels use a replace-all strategy during updates (delete all existing, insert all new) rather than incremental updates. This simplifies the update logic but may have performance implications for models with many labels.

### Route Validation
Azure IoT Edge routes use a specific query language syntax validated by regex. The system validates syntax but cannot verify semantic correctness (e.g., whether referenced modules exist) until cloud deployment.

### User Experience Considerations
- Name field is read-only after creation in the UI (technical limitation or business rule?)
- Tabbed interface reduces visual clutter for complex model configurations
- Expansion panels organize related settings logically
- Permission-based rendering ensures users only see actions they can perform
- Inline validation provides immediate feedback before submission
- Confirmation dialogs prevent accidental deletion

### Performance Optimization
- Pagination reduces initial page load for large model catalogs
- Server-side filtering reduces data transfer
- Avatar images loaded asynchronously to prevent blocking
- Repository queries use Include() for efficient label loading

### Future Considerations
- Model versioning and rollback capabilities for safer updates
- Configuration diff/comparison tools for troubleshooting
- Model cloning for rapid prototyping
- Import/export model configurations (JSON/YAML) for portability
- Module dependency validation and automatic ordering
- Draft/unpublished state for models under development
- Deployment success/failure tracking and analytics
- Automated rollback on deployment failure
- Container registry credential management in UI
- Visual route designer for complex routing scenarios

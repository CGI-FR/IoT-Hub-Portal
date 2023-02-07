# Diagrams

In order to better understand the needs of the project, here is a use case diagram regrouping the current use cases of the project.

## Connected objects

``` mermaid
graph LR
    A[End user] --> B(Display the list of connected objects)
    A --> C(Add a connected object)
    C -->|Extend| B
    D(Import a list of connected objects) -->|Extend| B
    E(Download a model) -->|Extend| B
    F(Export the list of connected objects) -->|Extend| B
    G(Delete a connected object) -->|Extend| B
    H(Go to the details of a connected object) -->|Extend| B
    I(Search for connected objects) -->|Extend| B
```

## Connected object models

``` mermaid
graph LR
    A[End user] --> J(Display the list of connected object models)
    A --> K(Add a connected object model)
    K -->|Extend| J
    L(Delete a connected object model) -->|Extend| J
    M(Go to the details of a connected object model) -->|Extend| J
```

## Connected object configurations

``` mermaid
graph LR
    A[End user] --> N(Display the list of connected object configurations)
    A --> O(Add a connected object configuration)
    O -->|Extend| N
    P(Go to the details of a connected object configuration) --> |Extend| N
```

## Edge connected object models

``` mermaid
graph LR
    A[End user] --> Q(Display the list of Edge connected object models)
    R(Add an Edge connected object model) -->|Extend| Q
    S(Delete an Edge connected object model) -->|Extend| Q
    T(Go to the details of a model of Edge connected object) -->|Extend| Q
    U(Search for Edge connected object models) -->|Extend| Q
```

## Edge connected objects

``` mermaid
graph LR
    A[End user] --> V(Display the list of Edge connected objects)
    W(Add an Edge connected object) -->|Extend| V
    X(Delete an Edge connected object) -->|Extend| V
    Y(Go to the details of a connected object Edge) -->|Extend| V
    Z(Search for Edge connected objects) -->|Extend| V
```

## Concentrators

``` mermaid
graph LR
    A[End user] --> AA(Display the list of concentrators)
    AB(Add a concentrator) -->|Extend| AA
    AC(Delete a concentrator) -->|Extend| AA
    AD(Go to the details of a concentrator) -->|Extend| AA
```

## Tags

``` mermaid
graph LR
    A[End user] --> AE(Display the list of tags)
    AF(Add a tag) -->|Extend| AE
    AG(Delete a tag) -->|Extend| AE
```

Now, here is a diagram representing the multilayer technical architecture of the project.

``` mermaid
C4Deployment
    title Multilayer technical architecture

    Deployment_Node(api, "Server", "API"){
        Container(controllers, "Controllers", "C#", "They are used to route HTTP requests, they call the methods of the services and they return the content of the HTTP response as well as a HTTP code.")
        Container(services, "Services", "C#", "They are used to define the business logic as to call the methods of the data access layer for example.")
    }

    Deployment_Node(bll, "Application", "BLL"){
        Container(iservices, "Services", C#, "This package represents the interfaces of the services.")
    }

    Deployment_Node(dal, "Infrastructure", "DAL"){
        Deployment_Node(uow, "UnitOfWork", "UOW"){
            Container(repositories, "Repositories", "C# and EntityFramework", "A repository represents all the data management methods of an entity of the project.")
        }
    }

    Deployment_Node(domain, "Domain", "Domain"){
        Container(entities, "Entities", "C#", "They are used as object representation of tables in a database.")
        Container(irepositories, "Repositories", "C#", "This package represents the interfaces of the repositories.")
    }

    Rel(iservices, services, "dependency", "")
    Rel(repositories, services, "dependency", "")
    Rel(entities, services, "dependency", "")
    Rel(entities, iservices, "dependency", "")
    Rel(iservices, repositories, "dependency", "")
    Rel(entities, repositories, "dependency", "")
```

Now, to better understand the technical architecture of the project, here is a class diagram representing it.

``` mermaid
classDiagram
    direction LR
    class AdminController{
        -String value
    }
    class DashboardController{
        -String value
    }
    class DeviceConfigurationsController{
        -String value
    }
    class DeviceModelControllerBase{
        -String value
    }
    class DeviceModelPropertiesController{
        -String value
    }
    class DeviceModelPropertiesControllerBase{
        -String value
    }
    class DeviceModelController{
        -String value
    }
    class DevicesController{
        -String value
    }
    class DevicesControllerBase{
        -String value
    }
    class DeviceTagSettingsController{
        -String value
    }
    class EdgeDevicesController{
        -String value
    }
    class EdgeModelsController{
        -String value
    }
    class IdeasController{
        -String value
    }
    class SettingsController{
        -String value
    }
    class LoRaWANCommandsController{
        -String value
    }
    class LoRaWANConcentratorsController{
        -String value
    }
    class LoRaWANDeviceModelsController{
        -String value
    }
    class LoRaWANDevicesController{
        -String value
    }
    class LoRaWANFrequencyPlansController{
        -String value
    }
    LoRaWANDeviceModelsController --|> DeviceModelsControllerBase
    LoRaWANDevicesController --|> DevicesControllerBase
    DeviceModelPropertiesController --|> DeviceModelPropertiesControllerBase
    DeviceModelsController --|> DeviceModelsControllerBase
    DevicesController --|> DevicesControllerBase
    class ConfigService{
        -String value
    }
    class DeviceConfigurationsService{
        -String value
    }
    class DeviceModelPropertiesService{
        -String value
    }
    class DeviceModelService{
        -String value
    }
    class DevicePropertyService{
        -String value
    }
    class DeviceService{
        -String value
    }
    class DeviceServiceBase{
        -String value
    }
    class DeviceTagService{
        -String value
    }
    class EdgeDevicesService{
        -String value
    }
    class EdgeModelService{
        -String value
    }
    class ExternalDeviceService{
        -String value
    }
    class IdeaService{
        -String value
    }
    class LoRaWANCommandService{
        -String value
    }
    class LoRaWANConcentratorService{
        -String value
    }
    class LoRaWanDeviceService{
        -String value
    }
    class SubmitIdeaRequest{
        -String value
    }
    DeviceService --|> DeviceServiceBase
    LoRaWanDeviceService --|> DeviceServiceBase
    class IConfigService
    <<interface>> IConfigService
    class IDeviceConfigurationsService
    <<interface>> IDeviceConfigurationsService
    class IDeviceModelPropertiesService
    <<interface>> IDeviceModelPropertiesService
    class IDeviceModelService
    <<interface>> IDeviceModelService
    class IDevicePropertyService
    <<interface>> IDevicePropertyService
    class IDeviceService
    <<interface>> IDeviceService
    class IDeviceTagService
    <<interface>> IDeviceTagService
    class IEdgeDevicesService
    <<interface>> IEdgeDevicesService
    class IEdgeModelService
    <<interface>> IEdgeModelService
    class IExternalDeviceService
    <<interface>> IExternalDeviceService
    class IIdeaService
    <<interface>> IIdeaService
    class ILoRaWANCommandService
    <<interface>> ILoRaWANCommandService
    class ILoRaWANConcentratorService
    <<interface>> ILoRaWANConcentratorService
    class ILoRaWanManagementService
    <<interface>> ILoRaWanManagementService
    ConfigService ..|> IConfigService
    DeviceConfigurationsService ..|> IDeviceConfigurationsService
    DeviceModelPropertiesService ..|> IDeviceModelPropertiesService
    DeviceModelService ..|> IDeviceModelService
    DevicePropertyService ..|> IDevicePropertyService
    DeviceServiceBase ..|> IDeviceService
    DeviceTagService ..|> IDeviceTagService
    EdgeDevicesService ..|> IEdgeDevicesService
    EdgeModelService ..|> IEdgeModelService
    ExternalDeviceService ..|> IExternalDeviceService
    IdeaService ..|> IIdeaService
    LoRaWANCommandService ..|> ILoRaWANCommandService
    LoRaWANConcentratorService ..|> ILoRaWANConcentratorService
    class ConcentratorRepository{
        -String value
    }
    class DeviceModelCommandRepository{
        -String value
    }
    class DeviceModelPropertiesRepository{
        -String value
    }
    class DeviceModelRepository{
        -String value
    }
    class DeviceRepository{
        -String value
    }
    class DeviceTagRepository{
        -String value
    }
    class DeviceTagValueRepository{
        -String value
    }
    class EdgeDeviceModelCommandRepository{
        -String value
    }
    class EdgeDeviceModelRepository{
        -String value
    }
    class EdgeDeviceRepository{
        -String value
    }
    class GenericRepository{
        -String value
    }
    class LabelRepository{
        -String value
    }
    class LoRaDeviceTelemetryRepository{
        -String value
    }
    class LorawanDeviceRepository{
        -String value
    }
    class UnitOfWork{
        -String value
    }
    class IConcentratorRepository
    <<interface>> IConcentratorRepository
    class IDeviceModelCommandRepository
    <<interface>> IDeviceModelCommandRepository
    class IDeviceModelPropertiesRepository
    <<interface>> IDeviceModelPropertiesRepository
    class IDeviceModelRepository
    <<interface>> IDeviceModelRepository
    class IDeviceRepository
    <<interface>> IDeviceRepository
    class IDeviceTagRepository
    <<interface>> IDeviceTagRepository
    class IDeviceTagValueRepository
    <<interface>> IDeviceTagValueRepository
    class IEdgeDeviceModelCommandRepository
    <<interface>> IEdgeDeviceModelCommandRepository
    class IEdgeDeviceModelRepository
    <<interface>> IEdgeDeviceModelRepository
    class IEdgeDeviceRepository
    <<interface>> IEdgeDeviceRepository
    class ILabelRepository
    <<interface>> ILabelRepository
    class ILoRaDeviceTelemetryRepository
    <<interface>> ILoRaDeviceTelemetryRepository
    class ILorawanDeviceRepository
    <<interface>> ILorawanDeviceRepository
    class IRepository
    <<interface>> IRepository
    class IUnitOfWork
    <<interface>> IUnitOfWork
    UnitOfWork ..|> IUnitOfWork
    ConcentratorRepository ..|> IConcentratorRepository
    ConcentratorRepository --|> GenericRepository
    DeviceModelCommandRepository ..|> IDeviceModelCommandRepository
    DeviceModelCommandRepository --|> GenericRepository
    DeviceModelPropertiesRepository ..|> IDeviceModelPropertiesRepository
    DeviceModelPropertiesRepository --|> GenericRepository
    DeviceModelRepository ..|> IDeviceModelRepository
    DeviceModelRepository --|> GenericRepository
    DeviceRepository ..|> IDeviceRepository
    DeviceRepository --|> GenericRepository
    DeviceTagRepository ..|> IDeviceTagRepository
    DeviceTagRepository --|> GenericRepository
    DeviceTagValueRepository ..|> IDeviceTagValueRepository
    DeviceTagValueRepository --|> GenericRepository
    EdgeDeviceModelCommandRepository ..|> IEdgeDeviceModelCommandRepository
    EdgeDeviceModelCommandRepository --|> GenericRepository
    EdgeDeviceModelRepository ..|> IEdgeDeviceModelRepository
    EdgeDeviceModelRepository --|> GenericRepository
    EdgeDeviceRepository ..|> IEdgeDeviceRepository
    EdgeDeviceRepository --|> GenericRepository
    GenericRepository ..|> IRepository
    LabelRepository ..|> ILabelRepository
    LabelRepository --|> GenericRepository
    LoRaDeviceTelemetryRepository ..|> ILoRaDeviceTelemetryRepository
    LoRaDeviceTelemetryRepository --|> GenericRepository
    LorawanDeviceRepository ..|> ILorawanDeviceRepository
    LorawanDeviceRepository --|> GenericRepository
```

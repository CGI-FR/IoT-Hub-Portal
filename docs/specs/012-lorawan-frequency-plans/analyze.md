# Feature: LoRaWAN Frequency Plans

**Category**: LoRaWAN Configuration  
**Status**: Analyzed  

---

## Description

The LoRaWAN Frequency Plans feature provides a centralized read-only registry of supported LoRaWAN frequency plans (also known as regional parameters) for the portal. Frequency plans define the radio frequency specifications, channel configurations, and regulatory parameters required for LoRaWAN communications in different geographical regions. The feature includes:

- Predefined list of 22 supported frequency plans covering major regions worldwide
- Frequency plans for Asia (AS_923_925), Europe (EU_863_870), China (CN_470_510), United States (US_902_928), and Australia (AU_915_928)
- Regional frequency band specifications including sub-bands (FSB) and regional parameters (RP)
- Frequency plan selection for LoRaWAN concentrator configuration
- Static, hard-coded frequency plan data (no database persistence required)
- Read-only API endpoint for client applications

This feature is essential for LoRaWAN network compliance and proper radio configuration, providing business value through:
- Ensuring regulatory compliance for LoRaWAN deployments in different regions
- Simplifying concentrator configuration by providing standardized frequency plan options
- Supporting multi-region LoRaWAN deployments with appropriate radio parameters
- Preventing misconfiguration by offering only validated frequency plan combinations

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANFrequencyPlansController.cs` (Lines 1-47)
  - **Snippet**: Main REST API controller for frequency plans
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/freqencyplans")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANFrequencyPlansController : ControllerBase
    {
        [HttpGet(Name = "GET LoRaWAN Frequency plans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize("concentrator:read")]
        public ActionResult<IEnumerable<FrequencyPlan>> GetFrequencyPlans()
        {
            return this.Ok(new[] {
                new FrequencyPlan { FrequencyPlanID = "AS_923_925_1", Name = "Asia 923-925 MHz, Group 1"},
                new FrequencyPlan { FrequencyPlanID = "EU_863_870", Name = "Europe 863-870 MHz"},
                new FrequencyPlan { FrequencyPlanID = "US_902_928_FSB_1", Name = "United States 902-928 MHz, FSB 1"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_1", Name = "Australia 915-928 MHz, FSB 1"},
                // ... 22 total frequency plans
            });
        }
    }
    ```

### Business Logic
- No dedicated service layer - frequency plans are returned directly from controller
- Static data embedded in controller method (no external service calls)
- No database operations required

### Data Access
- No database entities or repositories
- No persistence layer
- Static in-memory data only

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/FrequencyPlan.cs` (Lines 1-12)
  - **Snippet**: Frequency plan DTO
    ```csharp
    public class FrequencyPlan
    {
        public string FrequencyPlanID { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
    ```
  - Properties:
    - `FrequencyPlanID`: Unique identifier for the frequency plan (e.g., "EU_863_870", "US_902_928_FSB_1")
    - `Name`: Human-readable description of the frequency plan (e.g., "Europe 863-870 MHz", "United States 902-928 MHz, FSB 1")

### Client Services
- `src/IoTHub.Portal.Client/Services/ILoRaWanConcentratorClientService.cs` (Lines 18)
  - **Snippet**: Client service interface
    ```csharp
    public interface ILoRaWanConcentratorClientService
    {
        Task<IEnumerable<FrequencyPlan>> GetFrequencyPlans();
    }
    ```

- `src/IoTHub.Portal.Client/Services/LoRaWanConcentratorClientService.cs` (Lines 40-43)
  - **Snippet**: HTTP client implementation
    ```csharp
    public Task<IEnumerable<FrequencyPlan>> GetFrequencyPlans()
    {
        return this.http.GetFromJsonAsync<IEnumerable<FrequencyPlan>>("api/lorawan/freqencyplans")!;
    }
    ```
  - Note: URL contains typo "freqencyplans" (should be "frequencyplans")

### UI Components
- `src/IoTHub.Portal.Client/Pages/LoRaWAN/Concentrator/ConcentratorDetailPage.razor` (Lines 84-93)
  - Uses frequency plans in dropdown for region selection
  - **Snippet**: Frequency plan selector
    ```razor
    <MudSelect T="string" Label="Region" Variant="Variant.Outlined"
               @bind-Value="@concentrator.LoraRegion"
               Required="true"
               Disabled="@(!canWrite)">
        @foreach (var frequencyPlan in FrequencyPlans.OrderBy(c => c.Name))
        {
            <MudSelectItem Value="@(frequencyPlan.FrequencyPlanID)">@frequencyPlan.Name</MudSelectItem>
        }
    </MudSelect>
    ```

- `src/IoTHub.Portal.Client/Pages/LoRaWAN/Concentrator/CreateConcentratorPage.razor` (Lines 61-67)
  - Uses frequency plans in dropdown for new concentrator creation
  - Displays ordered list of frequency plans by name
  - Binds FrequencyPlanID to concentrator.LoraRegion property

### Filters
- `src/IoTHub.Portal.Server/Filters/LoRaFeatureActiveFilterAttribute.cs` (Lines 1-23)
  - Applied via `[LoRaFeatureActiveFilter]` attribute
  - Validates that LoRa features are enabled in portal configuration
  - Returns HTTP 400 Bad Request if LoRa features are disabled

---

## API Endpoints

### Frequency Plans
- `GET /api/lorawan/freqencyplans` - Get all supported frequency plans
  - Returns: IEnumerable<FrequencyPlan>
  - Authorization: concentrator:read
  - Response: Array of 22 frequency plan objects
  - Note: URL contains typo "freqencyplans"

---

## Authorization

### Required Permissions
- **concentrator:read** - View frequency plans (required for GET endpoint)

### Authorization Implementation
- Base authorization: `[Authorize]` on controller class
- Method-level authorization: `[Authorize("concentrator:read")]` on GetFrequencyPlans method
- Feature gate: `[LoRaFeatureActiveFilter]` ensures LoRa features are enabled in portal configuration

---

## Dependencies

### Internal Feature Dependencies
- **LoRaWAN Concentrator Management** - Frequency plans are used when creating or editing concentrators
- **LoRa Feature Configuration** - Feature must be enabled via ConfigHandler.IsLoRaEnabled

### Service Dependencies
- `ConfigHandler` - Validates LoRa features are enabled (used by LoRaFeatureActiveFilter)

### External Dependencies
- None - frequency plans are static data, no external service calls

### UI Dependencies
- **MudBlazor** - UI component library (MudSelect, MudSelectItem)
- Used exclusively in LoRaWAN Concentrator pages for region selection

---

## Key Features & Behaviors

### Supported Frequency Plans

#### Asia Region (3 plans)
- AS_923_925_1 - Asia 923-925 MHz, Group 1
- AS_923_925_2 - Asia 923-925 MHz, Group 2
- AS_923_925_3 - Asia 923-925 MHz, Group 3

#### Europe Region (1 plan)
- EU_863_870 - Europe 863-870 MHz

#### China Region (2 plans)
- CN_470_510_RP1 - China 470-510 MHz, RP 1
- CN_470_510_RP2 - China 470-510 MHz, RP 2

#### United States Region (7 plans)
- US_902_928_FSB_1 through US_902_928_FSB_7 - United States 902-928 MHz, FSB 1-7
- FSB = Frequency Sub-Band (allows selection of specific 8-channel sub-bands within the full 64-channel US915 band)

#### Australia Region (8 plans)
- AU_915_928_FSB_1 through AU_915_928_FSB_8 - Australia 915-928 MHz, FSB 1-8
- FSB = Frequency Sub-Band (allows selection of specific 8-channel sub-bands within the full 64-channel AU915 band)

### Frequency Plan Selection
- Dropdown selection with human-readable names
- Sorted alphabetically by name for user convenience
- FrequencyPlanID value stored in concentrator configuration
- Required field for concentrator creation and editing

### Static Data Model
- No database persistence
- Hard-coded in controller method
- Returns same data on every request
- No create, update, or delete operations
- Read-only data source

---

## Notes

### Architecture Patterns
- **Static Reference Data** - Frequency plans are embedded as static data rather than stored in database
- **Controller-Only Implementation** - No service layer needed for simple static data
- **RESTful Read-Only Endpoint** - Single GET endpoint following REST conventions

### LoRaWAN Standards Compliance
- Frequency plans align with LoRaWAN Regional Parameters specification
- Supports major regulatory regions worldwide
- Sub-band configurations (FSB) allow compliance with regulatory restrictions in US and Australia
- Regional Parameters (RP) versions supported for China region

### URL Typo
- API route contains typo: `/api/lorawan/freqencyplans` (missing 'u')
- Consistent across client and server implementation
- Changing would be breaking change for existing API clients
- Documented here for awareness

### Design Decisions
- **Static vs Dynamic**: Frequency plans are LoRaWAN standard definitions that rarely change, justifying static implementation
- **No Database Storage**: Reduces complexity and improves performance for reference data
- **Embedded in Controller**: Simple approach appropriate for small, stable dataset
- **No CRUD Operations**: Frequency plans are part of LoRaWAN specification, not user-configurable

### Performance Considerations
- Minimal overhead - returns static array from memory
- No database queries required
- Fast response time for dropdown population
- All 22 frequency plans returned in single request (no pagination needed)

### Security Considerations
- Read-only endpoint prevents unauthorized modifications
- Requires authentication (`[Authorize]` attribute)
- Requires concentrator:read permission
- Protected by LoRa feature flag (must be enabled in configuration)

### Testing Coverage
- Unit test exists: `LoRaWANFrequencyPlansControllerTests.cs`
- Test validates controller returns valid ActionResult with frequency plan collection
- Client service tests validate HTTP calls in `LoRaWanConcentratorsClientServiceTests.cs`
- Integration tests in concentrator page tests verify frequency plan loading

### Multi-Region Support
- Comprehensive coverage of major LoRaWAN regions
- Supports both wide-band (Europe, Asia) and sub-band configurations (US, Australia)
- Enables global deployment scenarios
- Multiple options per region allow fine-tuned regulatory compliance

### Future Enhancement Opportunities
- Fix URL typo in new API version (v2.0)
- Add frequency plan metadata (actual channel frequencies, bandwidth, data rates)
- Support for additional emerging LoRaWAN regions
- Database-backed frequency plans for custom/private network configurations
- Frequency plan validation against concentrator hardware capabilities
- API endpoint for frequency plan details by ID
- Localization of frequency plan names

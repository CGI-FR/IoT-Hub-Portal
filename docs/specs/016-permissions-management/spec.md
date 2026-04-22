# Feature Specification: Permissions Management

**Feature ID**: 016  
**Feature Branch**: `016-permissions-management`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/016-permissions-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View Available Permissions (Priority: P1)

As a security administrator, I need to view all available portal permissions so that I can understand what capabilities exist and plan role configurations.

**Why this priority**: Understanding available permissions is foundational to creating effective roles and access policies.

**Independent Test**: Can be fully tested by requesting the permissions list and verifying all portal capabilities are returned.

**Acceptance Scenarios**:

1. **Given** I am configuring roles, **When** I request the available permissions list, **Then** I receive all portal permissions.
2. **Given** the permissions list is returned, **When** I examine it, **Then** permissions are in "resource:action" format (e.g., "device:read").
3. **Given** permissions are needed before login, **When** I request the list, **Then** it is available without authentication for UI initialization.

---

### User Story 2 - View My Permissions (Priority: P1)

As an authenticated user, I need to know what permissions I have so that I can understand what actions I'm authorized to perform in the portal.

**Why this priority**: Users need to understand their capabilities; the UI uses this to show/hide features appropriately.

**Independent Test**: Can be fully tested by logging in with various roles and verifying the correct permissions are returned.

**Acceptance Scenarios**:

1. **Given** I am authenticated with roles, **When** I request my permissions, **Then** I receive all permissions from my assigned roles.
2. **Given** I have multiple roles, **When** I request my permissions, **Then** permissions from all roles are combined (union).
3. **Given** I have no role assignments, **When** I request my permissions, **Then** I receive an empty permission list.
4. **Given** I am not authenticated, **When** I request my permissions, **Then** I receive an unauthorized response.

---

### User Story 3 - Automatic User Provisioning on Permission Query (Priority: P2)

As a new user, I need my account to be created automatically when my permissions are first queried so that I can immediately access appropriate features.

**Why this priority**: Auto-provisioning during permission check ensures seamless onboarding for OAuth users.

**Independent Test**: Can be fully tested by having a new OAuth user query their permissions and verifying a user account is created.

**Acceptance Scenarios**:

1. **Given** I am a new user authenticated via OAuth, **When** my permissions are queried, **Then** my user account is automatically created.
2. **Given** my account is auto-created, **When** I next access the portal, **Then** I can be assigned roles without re-authentication.

---

### Edge Cases

- What happens if the email claim is missing from the OAuth token? (Unauthorized response with warning logged)
- How are permissions cached for performance? (Currently not cached; considered for future enhancement)
- What happens when a user's roles change while they're logged in? (Permissions refresh on next query)
- How are duplicate permissions handled across multiple roles? (Duplicates are de-duplicated in response)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide a list of all available portal permissions
- **FR-002**: System MUST allow anonymous access to the full permissions list (for pre-auth UI)
- **FR-003**: System MUST return the current user's effective permissions
- **FR-004**: System MUST require authentication for user-specific permission queries
- **FR-005**: System MUST identify users by email claim from OAuth token
- **FR-006**: System MUST auto-create user accounts on first permission query
- **FR-007**: System MUST combine permissions from all assigned roles for a user
- **FR-008**: Permissions MUST be returned as an array of permission identifiers
- **FR-009**: System MUST log permission query activity for audit purposes

### Key Entities

- **PortalPermissions**: Static enumeration of all available permissions:
  - Device operations: device:read, device:write, device:execute, device:export, device:import
  - Edge device operations: edge-device:read, edge-device:write
  - Model operations: model:read, model:write
  - Concentrator operations: concentrator:read, concentrator:write
  - User operations: user:read, user:write
  - Role operations: role:read, role:write
  - Access control operations: access-control:read, access-control:write
  - Planning operations: planning:read, planning:write
  - Schedule operations: schedule:read, schedule:write
  - Layer operations: layer:read, layer:write
  - Dashboard operations: dashboard:read
  - Setting operations: setting:read
  - Idea operations: idea:write

### Permission Format

Permissions follow the pattern: `{resource}:{action}`

| Resource | Actions |
|----------|---------|
| device | read, write, execute, export, import |
| edge-device | read, write |
| model | read, write |
| concentrator | read, write |
| user | read, write |
| role | read, write |
| access-control | read, write |
| planning | read, write |
| schedule | read, write |
| layer | read, write |
| dashboard | read |
| setting | read |
| idea | write |

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Permission list retrieval completes in under 100 milliseconds
- **SC-002**: User permission queries complete in under 500 milliseconds
- **SC-003**: 100% of authenticated users can query their permissions successfully
- **SC-004**: UI correctly shows/hides features based on user permissions in all cases
- **SC-005**: Zero security incidents due to incorrect permission reporting

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/016-permissions-management/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- PermissionsController: Permission query endpoints
- PortalPermissionsHelper: Static permission definitions
- UserManagementService: User lookup and auto-provisioning
- AccessControlManagementService: Permission verification

### Dependencies
- **Depends On**: 
  - 013-user-management (user identification and creation)
  - 015-access-control-management (permission verification)
  - 014-role-management (roles contain permissions)
- **Depended By**: 
  - All portal UI components (use permissions for conditional rendering)
  - All protected API endpoints (use permissions for authorization)

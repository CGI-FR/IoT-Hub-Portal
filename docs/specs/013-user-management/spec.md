# Feature Specification: User Management

**Feature ID**: 013  
**Feature Branch**: `013-user-management`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/013-user-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View User Directory (Priority: P1)

As an administrator, I need to view a paginated list of all portal users so that I can monitor who has access to the system and manage user accounts effectively.

**Why this priority**: User visibility is the foundation of user management - administrators must see who has access before they can manage accounts.

**Independent Test**: Can be fully tested by navigating to the user list page and verifying users are displayed with pagination, search, and sorting.

**Acceptance Scenarios**:

1. **Given** multiple users exist in the system, **When** I access the user directory, **Then** I see a paginated list showing user names, emails, and principal IDs.
2. **Given** I am viewing the user list, **When** I enter a name in the search field, **Then** the list filters to show only matching users.
3. **Given** I am viewing the user list, **When** I enter an email in the search field, **Then** the list filters to show only matching email addresses.
4. **Given** more users exist than the page size, **When** I navigate between pages, **Then** I can view all users across multiple pages.
5. **Given** I want to sort users, **When** I click on a column header, **Then** the list is sorted by that column.

---

### User Story 2 - View User Details (Priority: P1)

As an administrator, I need to view detailed information about a specific user so that I can understand their profile and current role assignments.

**Why this priority**: Detailed user information is essential for troubleshooting access issues and verifying correct role assignments.

**Independent Test**: Can be fully tested by selecting a user from the list and viewing their complete profile including role assignments.

**Acceptance Scenarios**:

1. **Given** I select a user from the directory, **When** the detail page loads, **Then** I see their full profile (given name, family name, email, avatar).
2. **Given** I am viewing a user's details, **When** I check their role assignments, **Then** I see all roles currently assigned to them with their scopes.
3. **Given** I am viewing a user without any role assignments, **When** I view their roles section, **Then** I see an empty state indicating no roles assigned.

---

### User Story 3 - Create New User (Priority: P2)

As an administrator, I need to create new user accounts manually so that I can pre-provision access for team members before their first login.

**Why this priority**: While most users are auto-provisioned on first OAuth login, pre-provisioning enables proactive access management.

**Independent Test**: Can be fully tested by filling out the new user form, submitting it, and verifying the user appears in the directory.

**Acceptance Scenarios**:

1. **Given** I have administrator privileges, **When** I complete the new user form with valid information, **Then** the user is created and appears in the directory.
2. **Given** I am creating a user, **When** I submit a duplicate username, **Then** an error indicates the name is already taken.
3. **Given** I have created a new user, **When** the creation succeeds, **Then** a principal is automatically created for role assignments.

---

### User Story 4 - Modify User Information (Priority: P2)

As an administrator, I need to update user profile information so that I can correct errors or update details as they change.

**Why this priority**: Maintaining accurate user information is important for communication and identification, but less frequent than viewing.

**Independent Test**: Can be fully tested by editing a user's profile, saving changes, and verifying the updates are persisted.

**Acceptance Scenarios**:

1. **Given** I am viewing a user's details, **When** I modify their profile fields and save, **Then** the changes are persisted.
2. **Given** I am editing a user, **When** I change their name to one already in use, **Then** an error prevents the duplicate.

---

### User Story 5 - Delete User Account (Priority: P3)

As an administrator, I need to remove user accounts so that I can revoke access for users who should no longer have portal access.

**Why this priority**: User deletion is less frequent but essential for security and access control compliance.

**Independent Test**: Can be fully tested by deleting a user and verifying they no longer appear in the directory and cannot access the system.

**Acceptance Scenarios**:

1. **Given** I select a user to delete, **When** I confirm the deletion, **Then** the user is removed from the directory.
2. **Given** I delete a user, **When** the deletion completes, **Then** their associated principal and access controls are also removed.
3. **Given** a deleted user attempts to log in via OAuth, **When** they authenticate, **Then** a new user record is created (fresh start).

---

### User Story 6 - Automatic User Provisioning (Priority: P1)

As a new user, I need to have my account automatically created when I first log in via OAuth so that I can immediately access the portal based on my authentication.

**Why this priority**: Auto-provisioning is critical for seamless user onboarding and reduces administrative overhead.

**Independent Test**: Can be fully tested by having a new user authenticate via OAuth and verifying their account is created with correct profile data.

**Acceptance Scenarios**:

1. **Given** I am a new user authenticating via OAuth, **When** I successfully authenticate, **Then** my user account is automatically created with profile data from OAuth claims.
2. **Given** I am the first user in an empty system, **When** I log in, **Then** I am automatically assigned administrator privileges.
3. **Given** my OAuth profile has email, name, and avatar, **When** my account is created, **Then** these fields are populated from my OAuth claims.

---

### Edge Cases

- What happens when a user's email changes in the OAuth provider? (User is matched by email claim; if email changes, a new account is created)
- How are duplicate emails handled if created manually vs. OAuth? (Email must be unique; manual creation should validate against existing accounts)
- What happens to a user's data and access when they are deleted? (All access controls are cascade deleted; user cannot access portal)
- How is the "first administrator" determined in a fresh installation? (First user to successfully authenticate becomes admin)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST display a paginated list of all users with configurable page sizes
- **FR-002**: System MUST support searching users by name (given name, family name, or full name)
- **FR-003**: System MUST support searching users by email address
- **FR-004**: System MUST support sorting the user list by name and email columns
- **FR-005**: System MUST display user details including given name, family name, email, and avatar
- **FR-006**: System MUST allow creating new user accounts with profile information
- **FR-007**: System MUST validate that usernames are unique across the system
- **FR-008**: System MUST automatically create a principal entity when a new user is created
- **FR-009**: System MUST allow updating user profile information
- **FR-010**: System MUST allow deleting user accounts with cascade removal of associated data
- **FR-011**: System MUST automatically provision new users on first OAuth authentication
- **FR-012**: System MUST extract user profile from OAuth claims (email, name, avatar)
- **FR-013**: System MUST assign administrator role to the first user in an empty system

### Key Entities

- **User**: Core user account containing:
  - Email (unique identifier, from OAuth claim)
  - GivenName (first name)
  - FamilyName (last name)
  - Name (full display name)
  - Avatar (profile image URL)
  - PrincipalId (link to RBAC principal)

- **Principal**: RBAC identity for access control:
  - Associated User (one-to-one)
  - AccessControls collection (role assignments)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Administrators can find any user in the directory within 30 seconds using search
- **SC-002**: New users are automatically provisioned and can access appropriate features within 5 seconds of first OAuth login
- **SC-003**: 100% of deleted users have their access immediately revoked
- **SC-004**: User list pages load within 2 seconds even with 1000+ users
- **SC-005**: First administrator auto-assignment occurs 100% of the time in fresh installations

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/013-user-management/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- UsersController: User CRUD endpoints
- UserService: Business logic for user operations
- UserRepository: Data access for user entities
- UsersListPage.razor: User directory UI
- UserDetailPage.razor: User details and role management UI

### Dependencies
- **Depends On**: 
  - OAuth/OIDC Authentication (external identity provider)
  - 015-access-control-management (role assignments)
- **Depended By**: 
  - 014-role-management (roles are assigned to users)
  - 015-access-control-management (access controls reference user principals)
  - 016-permissions-management (permissions are checked against user context)

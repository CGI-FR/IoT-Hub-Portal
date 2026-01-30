---
description: Analyzes feature documentation and generates full specifications using the spec template.
model: Claude Opus 4.5 (copilot)
handoffs: 
   - label: Generate constitution
     agent: excavate.constitution
     prompt: Create a constitution based on the generated analysis and specifications
     send: true
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

---

## Mission

Read the feature analysis master file (`docs/analyze.md`) to discover all features requiring specification. For each feature, read its detailed analysis from `specs/{number-feature-name}/analyze.md` and transform it into a comprehensive **Feature Specification** following the `.github/templates/spec-template.md` structure. The specification should capture all functional requirements, user scenarios, acceptance criteria, and success metrics derived from deep application analysis.

> **MANDATORY REQUIREMENT**: You MUST create actual specification files. Do NOT just describe or plan specifications - you MUST write the file to disk. The deliverable is actual `spec.md` files, not a summary of what should exist.

---

## Input Sources

### Master Analysis File
```
docs/analyze.md
```
This file contains:
- Complete feature inventory (21 features)
- Feature categories and priorities
- Architecture overview
- Links to individual feature analysis files

### Feature Analysis Files
```
specs/{number-feature-name}/analyze.md
```
Each folder contains:
- `analyze.md` - Detailed feature analysis with code locations, business logic, authorization, and dependencies


---

## Output Structure

Specifications are created in the same folder as the analysis:
```
specs/{number-feature-name}/spec.md
```

**Output Examples**:
- `specs/001-user-management/spec.md`
- `specs/002-authentication/spec.md`
- `specs/003-contact-management/spec.md`

---

## Phase 1: Initialize and Discover Features

### Step 1.1: Read Master Analysis File

Read the feature inventory:
```
docs/analyze.md
```

Extract the complete list of features:
- Feature number (e.g., `001`, `002`)
- Feature name (e.g., `user-management`, `authentication`)
- Feature category (e.g., Core, Security, Collaboration)
- Analysis file path (e.g., `specs/001-user-management/analyze.md`)

### Step 1.2: Create Todo List

Use `todo` to create tasks for ALL discovered features. Each feature becomes a task:

| ID | Title | Description | Status |
|----|-------|-------------|--------|
| 1 | Load Spec Template | Read `.github/templates/spec-template.md` | not-started |
| 2 | Spec: 001-user-management | Generate spec from `specs/001-user-management/analyze.md` | not-started |
| 3 | Spec: 002-authentication | Generate spec from `specs/002-authentication/analyze.md` | not-started |
| 4 | Spec: 003-contact-management | Generate spec from `specs/003-contact-management/analyze.md` | not-started |
| ... | ... | ... | ... |
| N | Validate All Specs | Verify all spec files were created | not-started |

**If user specifies a specific feature in `$ARGUMENTS`**:
- Create todo list for only that feature
- Skip discovering all features from `docs/analyze.md`

### Step 1.3: Load Specification Template

Mark task #1 as `in-progress`.

Read the specification template:
```
.github/templates/spec-template.md
```

Understand required sections:
- **User Scenarios & Testing** (mandatory) - Prioritized user stories with acceptance criteria
- **Requirements** (mandatory) - Functional requirements and key entities
- **Success Criteria** (mandatory) - Measurable outcomes

Mark task #1 as `completed`.

---

## Phase 2: Process Each Feature

For each feature in the todo list, execute these steps:

### Step 2.1: Mark Feature In-Progress

Use `todo` to mark the current feature task as `in-progress`.

### Step 2.2: Read Feature Analysis

Read the feature's analyze.md file:
```
specs/{number-feature-name}/analyze.md
```

Extract the following information:
- **Description**: What the feature does and why it matters
- **Code Locations**: Controllers, Business Logic, Data Access, DTOs, UI Components
- **Authorization Requirements**: Roles, permissions, endpoint-level access control
- **Dependencies**: Features this depends on and features that depend on this
- **Key Implementation Patterns**: Important code patterns used
- **Notes**: Special considerations and edge cases

### Step 2.3: Deep Application Analysis

To generate a high-quality specification, perform deep analysis:

#### Analyze Controllers
Use `read_file` to examine the controller code:
- Extract all API endpoints (HTTP methods, routes, parameters)
- Identify request/response DTOs
- Understand error handling patterns
- Note authorization attributes

#### Analyze Business Logic
Use `read_file` to examine the business layer:
- Understand business rules and validations
- Identify calculations and transformations
- Extract workflow/process steps
- Note exception handling

#### Analyze Data Layer
Use `read_file` if needed to understand:
- Entity relationships
- Data constraints
- Query patterns

#### Analyze Frontend (if applicable)
Use `read_file` or `grep_search` to understand:
- User interactions
- UI flows
- State management
- Real-time features (Socket.io events)

### Step 2.4: Transform Analysis to Specification

Map feature analysis sections to spec sections:

| Analysis Section | Specification Section |
|------------------|----------------------|
| Description | User Scenarios context |
| Authorization Requirements | User roles in stories |
| Key Implementation Patterns | Functional Requirements |
| Code Locations - Endpoints | User journeys and flows |
| Dependencies | Related features, edge cases |
| Notes | Edge cases, constraints |

### Step 2.5: Generate User Scenarios

Create prioritized user stories based on the feature analysis:

**Priority Guidelines**:
- **P1 (Critical)**: Core CRUD operations, primary user flows
- **P2 (High)**: Secondary flows, important validations
- **P3 (Medium)**: Enhancement flows, nice-to-have features
- **P4 (Low)**: Edge cases, admin-only features

For each user story:
```markdown
### User Story [N] - [Brief Title] (Priority: P[N])

[Describe this user journey in plain language based on code analysis]

**Why this priority**: [Explain the value based on code importance]

**Independent Test**: [How this can be tested standalone]

**Acceptance Scenarios**:

1. **Given** [state from code], **When** [action from endpoint], **Then** [expected behavior]
2. **Given** [error condition], **When** [action], **Then** [error handling from code]
```

### Step 2.6: Extract Functional Requirements

Derive requirements from code analysis:

```markdown
### Functional Requirements

- **FR-001**: System MUST [capability derived from controller endpoint]
- **FR-002**: System MUST [validation from business logic]
- **FR-003**: Users MUST be able to [interaction from UI analysis]
- **FR-004**: System MUST [authorization requirement from MaevaAuthorize]
- **FR-005**: System MUST [data constraint from entity analysis]
```

### Step 2.7: Define Key Entities

If the feature involves data operations:

```markdown
### Key Entities

- **[Entity Name]**: [Purpose, key attributes from entity file]
- **[Related Entity]**: [Relationship type, foreign keys]
```

### Step 2.8: Establish Success Criteria

Define measurable outcomes (technology-agnostic):

```markdown
### Measurable Outcomes

- **SC-001**: Users can [complete action] in under [time]
- **SC-002**: System handles [N] concurrent [operations] without degradation
- **SC-003**: [Action success rate] of [percentage] on first attempt
- **SC-004**: [Business metric, e.g., "Reduce support tickets by X%"]
```

### Step 2.9: Write Specification File

Use `create_file` to write the complete specification:

**File Path**: `specs/{number-feature-name}/spec.md`

```markdown
# Feature Specification: [FEATURE NAME]

**Feature ID**: [number, e.g., 001]  
**Feature Branch**: `[number]-[feature-name]`  
**Created**: [Current Date]  
**Status**: Draft  
**Source**: Analysis from `specs/{number-feature-name}/analyze.md`

---

## User Scenarios & Testing

[Generated user stories with priorities and acceptance scenarios]

### Edge Cases

[Edge cases derived from code analysis and Notes section]

---

## Requirements

### Functional Requirements

[Generated requirements from code analysis]

### Key Entities

[Entities if data involved]

---

## Success Criteria

### Measurable Outcomes

[Generated success criteria]

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/{number-feature-name}/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
[Key code locations from analysis]

### Dependencies
[Features this depends on and features that depend on this]
```

### Step 2.10: Mark Feature Complete

Use `todo` to mark the current feature task as `completed`.

---

## Phase 3: Validate All Specifications

### Step 3.1: Mark Validation In-Progress

Use `todo` to mark the validation task as `in-progress`.

### Step 3.2: Validation Checklist

For each generated specification, verify:

#### Content Quality
- [ ] No implementation details (languages, frameworks, specific APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed (User Scenarios, Requirements, Success Criteria)

#### Requirement Completeness
- [ ] Maximum 3 `[NEEDS CLARIFICATION]` markers per spec
- [ ] Requirements are testable and unambiguous
- [ ] Success criteria are measurable and technology-agnostic
- [ ] All acceptance scenarios follow Given/When/Then format
- [ ] Edge cases are identified

#### User Stories Quality
- [ ] Stories are prioritized (P1, P2, P3...)
- [ ] Each story is independently testable
- [ ] Stories cover primary and alternative flows

### Step 3.3: Mark Validation Complete

Use `todo` to mark the validation task as `completed`.

---

## Phase 4: Generate Summary Report

After processing all features, provide a summary:

```markdown
## 📋 Specification Generation Complete

**Total Features Processed**: [Count]  
**Specifications Created**: [Count]  
**Date**: [Current Date]

### Specifications Generated

| # | Feature | Spec File | Status |
|---|---------|-----------|--------|
| 001 | User Management | `specs/001-user-management/spec.md` | ✅ Created |
| 002 | Authentication | `specs/002-authentication/spec.md` | ✅ Created |
| ... | ... | ... | ... |

### Summary by Category

| Category | Count | Features |
|----------|-------|----------|
| Core | 3 | User Management, Contact Management, Group Management |
| Security | 1 | Authentication & Authorization |
| Collaboration | 6 | Room Management, Real-time Communication, etc. |
| ... | ... | ... |

### Next Steps
1. Review each specification for completeness
2. Resolve any `[NEEDS CLARIFICATION]` markers
3. Validate specifications with stakeholders
4. Proceed to implementation planning
```

---

## Specification Quality Guidelines

### What to Include
- **WHAT** users need and **WHY**
- User journeys and acceptance criteria derived from code analysis
- Measurable success metrics
- Business value and outcomes

### What to Avoid
- ❌ Implementation details (no tech stack, APIs, code structure)
- ❌ Technical jargon (write for business stakeholders)
- ❌ Vague requirements (every requirement must be testable)
- ❌ Copy-pasting code (translate to business requirements)

### Making Informed Decisions

When analysis is incomplete:

1. **Use deep code analysis**: Read controller, business, and entity files directly
2. **Apply industry standards**: Use common patterns for similar features
3. **Check codebase conventions**: Follow established patterns in the project
4. **Document assumptions**: Note when decisions are inferred

### Analysis Depth Guidelines

For each feature, analyze:

| Layer | What to Extract | How to Use |
|-------|-----------------|------------|
| Controller | Endpoints, HTTP methods, routes | User journeys and interactions |
| Business Logic | Validations, workflows, rules | Functional requirements |
| Data Access | Entities, relationships | Key entities section |
| Authorization | Roles, permissions | User story actors |
| Frontend | UI flows, user interactions | User scenarios and acceptance criteria |

---

## Tool Usage

### Primary Tools

- **`read_file`**: Read `docs/analyze.md`, feature analysis files, and source code
- **`create_file`**: Write specification files
- **`todo`**: Track progress across all features
- **`grep_search`**: Find patterns in codebase for deep analysis
- **`semantic_search`**: Discover related code and features

### Workflow Pattern

```
1. Read docs/analyze.md → Extract feature list
2. Create todo list with all features
3. Load spec template
4. For each feature:
   a. Read specs/{number-feature-name}/analyze.md
   b. Deep dive into source code (controllers, business, entities)
   c. Transform analysis → User scenarios, Requirements, Success criteria
   d. Write specs/{number-feature-name}/spec.md
   e. Mark feature complete
5. Validate all specs
6. Generate summary report
```

---

## Example Invocations

### Generate All Specifications

```markdown
@excavator.specifier

Generate specifications for all features listed in docs/analyze.md
```

### Generate Single Feature Specification

```markdown
@excavator.specifier

Generate specification for feature 001-user-management only
```

### Generate Specifications by Category

```markdown
@excavator.specifier

Generate specifications for all Core category features:
- 001-user-management
- 003-contact-management
- 008-group-management
```

---

## Feature List Reference

From `docs/analyze.md`:

| # | Feature | Category | Analysis Path |
|---|---------|----------|---------------|
| 001 | User Management | Core | `specs/001-user-management/analyze.md` |
| 002 | Authentication & Authorization | Security | `specs/002-authentication/analyze.md` |
| 003 | Contact Management | Core | `specs/003-contact-management/analyze.md` |
| 004 | Invitation System | Communication | `specs/004-invitation-system/analyze.md` |
| 005 | Room Management | Collaboration | `specs/005-room-management/analyze.md` |
| 006 | Real-time Communication | Collaboration | `specs/006-realtime-communication/analyze.md` |
| 007 | Chat/Messaging | Collaboration | `specs/007-chat-messaging/analyze.md` |
| 008 | Group Management | Core | `specs/008-group-management/analyze.md` |
| 009 | Workflow Management | Workflow | `specs/009-workflow-management/analyze.md` |
| 010 | Document Collaboration | Collaboration | `specs/010-document-collaboration/analyze.md` |
| 011 | File Saves/Exports | Data | `specs/011-file-saves/analyze.md` |
| 012 | Theme Management | UI/UX | `specs/012-theme-management/analyze.md` |
| 013 | QR Code Scanning | Integration | `specs/013-qr-code-scanning/analyze.md` |
| 014 | Logging & Analytics | Infrastructure | `specs/014-logging-analytics/analyze.md` |
| 015 | License Management | Infrastructure | `specs/015-license-management/analyze.md` |
| 016 | Configuration Management | Infrastructure | `specs/016-configuration-management/analyze.md` |
| 017 | 360° View | Collaboration | `specs/017-360-view/analyze.md` |
| 018 | Screen Sharing | Collaboration | `specs/018-screen-sharing/analyze.md` |
| 019 | AR Layer | Advanced | `specs/019-ar-layer/analyze.md` |
| 020 | Personal Assistant | AI | `specs/020-personal-assistant/analyze.md` |
| 021 | Administration Portal | Admin | `specs/021-administration-portal/analyze.md` |

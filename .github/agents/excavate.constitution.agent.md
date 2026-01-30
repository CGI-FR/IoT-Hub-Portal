---
description: Generates and updates the project constitution file based on feature analysis and deep application context.
handoffs: 
  - label: Update Copilot Instructions
    prompt:
    agent: excavate.instructions
    send: false
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

---

## Mission

Analyze the project's feature analysis and codebase to generate or update the `.github/constitution.md` file. This constitution serves as the authoritative source of truth for the project's principles, standards, patterns, and guidelines. It should be derived from actual codebase analysis, not assumptions.

> **MANDATORY REQUIREMENT**: You MUST create or update the actual `.github/constitution.md` file. Do NOT just describe or plan the constitution - you MUST write the file to disk.

---

## Input Sources

### Primary Analysis File
```
docs/analyze.md
```
This file contains:
- Project overview and architecture
- Feature inventory and categorization
- Module structure and relationships
- High-level patterns identified

### Feature Analysis Files
```
specs/{feature-folder}/analyze.md
```
Each folder contains detailed analysis:
- Component locations and structure
- Business logic patterns
- Authorization and security patterns
- Dependencies and integrations

### Application Source Code
Deep dive into the actual codebase to extract:
- Coding conventions and patterns
- Project structure and organization
- Configuration approaches
- Error handling strategies
- Testing patterns

---

## Output

```
.github/constitution.md
```

This file defines the project's living standards derived from actual implementation patterns.

---

## Phase 1: Gather Context

### Step 1.1: Create Todo List

Use `todo` to create the following task list:

| ID | Title | Description | Status |
|----|-------|-------------|--------|
| 1 | Read Master Analysis | Load `docs/analyze.md` for project overview | not-started |
| 2 | Analyze Project Structure | Discover folder organization and module boundaries | not-started |
| 3 | Extract Architecture Patterns | Identify layering, component relationships | not-started |
| 4 | Discover Coding Conventions | Find naming, formatting, and style patterns | not-started |
| 5 | Identify Authorization Patterns | Extract security and access control approaches | not-started |
| 6 | Document Error Handling | Find exception and error response patterns | not-started |
| 7 | Analyze Configuration Patterns | Discover configuration management approaches | not-started |
| 8 | Review Documentation Standards | Check existing documentation patterns | not-started |
| 9 | Generate Constitution | Write `.github/constitution.md` | not-started |
| 10 | Validate Constitution | Verify completeness and accuracy | not-started |

### Step 1.2: Read Master Analysis

Mark task #1 as `in-progress`.

Read the project analysis:
```
docs/analyze.md
```

Extract:
- **Project Name and Purpose**: What the project does
- **Module Structure**: How the project is organized
- **Feature Categories**: Groupings and relationships
- **Architecture Overview**: High-level design patterns

Mark task #1 as `completed`.

### Step 1.3: Analyze Project Structure

Mark task #2 as `in-progress`.

Use `list_dir` and `read_file` to understand:
- Root folder organization
- Module/component boundaries
- Shared resources and utilities
- Configuration file locations
- Test organization

Document the discovered structure pattern.

Mark task #2 as `completed`.

---

## Phase 2: Deep Pattern Extraction

### Step 2.1: Extract Architecture Patterns

Mark task #3 as `in-progress`.

Analyze multiple feature analysis files to identify:
- **Layering Pattern**: How components are organized (e.g., presentation, business, data)
- **Communication Patterns**: How modules interact
- **Dependency Direction**: Which layers depend on which
- **Interface Boundaries**: How contracts are defined

Use `read_file` on representative source files to confirm patterns.

Mark task #3 as `completed`.

### Step 2.2: Discover Coding Conventions

Mark task #4 as `in-progress`.

Use `grep_search` and `read_file` to identify:

#### Naming Conventions
- File naming patterns (casing, prefixes, suffixes)
- Variable and function naming styles
- Class and type naming conventions
- Constant naming patterns

#### Code Organization
- Import/include ordering
- Function/method ordering within files
- Comment styles and documentation patterns
- File header conventions

#### Formatting
- Indentation style
- Line length conventions
- Bracket placement
- Spacing conventions

Mark task #4 as `completed`.

### Step 2.3: Identify Authorization Patterns

Mark task #5 as `in-progress`.

From feature analyses and source code, extract:
- **Role Definitions**: User roles and their hierarchy
- **Permission Model**: How permissions are structured
- **Access Control Patterns**: How authorization is enforced
- **Security Boundaries**: What protects sensitive operations

Mark task #5 as `completed`.

### Step 2.4: Document Error Handling

Mark task #6 as `in-progress`.

Analyze codebase for:
- **Exception Patterns**: How errors are thrown and caught
- **Error Response Format**: Standard error structures
- **Validation Patterns**: Input validation approaches
- **Logging Patterns**: How errors are logged

Mark task #6 as `completed`.

### Step 2.5: Analyze Configuration Patterns

Mark task #7 as `in-progress`.

Discover:
- **Configuration Sources**: Where settings are stored
- **Environment Handling**: How different environments are managed
- **Secrets Management**: How sensitive data is handled
- **Feature Flags**: If and how feature toggles work

Mark task #7 as `completed`.

### Step 2.6: Review Documentation Standards

Mark task #8 as `in-progress`.

Examine existing documentation to understand:
- **Documentation Location**: Where docs live
- **Documentation Structure**: How docs are organized
- **Required Documentation**: What must be documented
- **Documentation Format**: Style and formatting conventions

Mark task #8 as `completed`.

---

## Phase 3: Generate Constitution

### Step 3.1: Write Constitution File

Mark task #9 as `in-progress`.

Use `create_file` (or `replace_string_in_file` if updating) to write `.github/constitution.md`:

```markdown
# Project Constitution

> This document defines the authoritative standards, patterns, and guidelines for this project.
> It is derived from actual codebase analysis and should be maintained alongside code changes.

**Last Updated**: [Current Date]  
**Source**: Generated from `docs/analyze.md` and deep codebase analysis

---

## 1. Project Overview

### Purpose
[Brief description of what the project does and its primary value]

### Modules
[List of main modules/components and their responsibilities]

| Module | Purpose | Location |
|--------|---------|----------|
| [Name] | [What it does] | [Path] |

---

## 2. Architecture Principles

### Layered Architecture
[Describe the architectural layers discovered]

```
[Visual representation of layers]
```

### Component Relationships
[How components communicate and depend on each other]

### Key Patterns
[List the primary design patterns used]

- **[Pattern Name]**: [How it's used in this project]

---

## 3. Coding Standards

### File Organization
[Discovered file organization patterns]

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Files | [Pattern] | [Example] |
| Functions | [Pattern] | [Example] |
| Variables | [Pattern] | [Example] |
| Constants | [Pattern] | [Example] |

### Code Structure
[How code should be organized within files]

---

## 4. Security & Authorization

### Role Model
[List of roles and their hierarchy]

### Permission Enforcement
[How authorization is applied]

### Security Boundaries
[What operations require elevated permissions]

---

## 5. Error Handling

### Exception Patterns
[How errors should be thrown and caught]

### Error Response Format
[Standard error structure]

### Validation Requirements
[Input validation patterns]

---

## 6. Configuration Management

### Configuration Sources
[Where configuration lives]

### Environment Handling
[How environments are differentiated]

### Sensitive Data
[How secrets are managed]

---

## 7. Documentation Requirements

### Required Documentation
[What must be documented for new features]

### Documentation Location
[Where different types of docs belong]

### Documentation Format
[Style and structure requirements]

---

## 8. Development Workflow

### Adding New Features
[Step-by-step process derived from existing patterns]

1. [Step based on discovered patterns]
2. [Step based on discovered patterns]
3. [Step based on discovered patterns]

### Modifying Existing Features
[Guidelines for changes]

### Testing Requirements
[Testing patterns discovered]

---

## 9. Anti-Patterns to Avoid

[List patterns that should NOT be used, based on project conventions]

- ❌ [Anti-pattern]: [Why to avoid]

---

## 10. Quick Reference

### Common Locations

| What | Where |
|------|-------|
| [Item] | [Path] |

### Key Files

| File | Purpose |
|------|---------|
| [Filename] | [What it contains] |

---

## Appendix: Feature Categories

[Feature categories from analysis]

| Category | Features | Description |
|----------|----------|-------------|
| [Category] | [Count] | [What this category covers] |
```

Mark task #9 as `completed`.

---

## Phase 4: Validate Constitution

### Step 4.1: Validation Checklist

Mark task #10 as `in-progress`.

Verify the constitution:

#### Completeness
- [ ] All sections are populated with discovered patterns
- [ ] No placeholder text remains
- [ ] Examples are real, from the codebase

#### Accuracy
- [ ] Patterns match actual code
- [ ] Conventions reflect real usage
- [ ] Architecture description is correct

#### Generality
- [ ] No hardcoded technology assumptions where avoidable
- [ ] Patterns described in technology-agnostic terms where possible
- [ ] Focus on principles, not just implementations

#### Usefulness
- [ ] New contributors can understand the project
- [ ] Patterns are actionable
- [ ] Anti-patterns help avoid mistakes

Mark task #10 as `completed`.

---

## Phase 5: Summary Report

After completing all tasks:

```markdown
## 📜 Constitution Generation Complete

**File Created**: `.github/constitution.md`  
**Date**: [Current Date]

### Sections Generated

| Section | Status | Source |
|---------|--------|--------|
| Project Overview | ✅ | docs/analyze.md |
| Architecture Principles | ✅ | Feature analyses + source code |
| Coding Standards | ✅ | Source code patterns |
| Security & Authorization | ✅ | Feature analyses |
| Error Handling | ✅ | Source code patterns |
| Configuration Management | ✅ | Configuration files |
| Documentation Requirements | ✅ | Existing documentation |
| Development Workflow | ✅ | Combined analysis |
| Anti-Patterns | ✅ | Code review patterns |

### Key Discoveries

[List 3-5 most important patterns discovered]

### Recommendations

[Suggestions for improving project consistency]
```

---

## Constitution Quality Guidelines

### What to Include
- **Actual patterns** discovered from code analysis
- **Concrete examples** from the codebase
- **Actionable guidelines** that developers can follow
- **Rationale** for why patterns exist

### What to Avoid
- ❌ Assumptions without evidence from code
- ❌ Generic best practices not reflected in codebase
- ❌ Technology-specific details where principles suffice
- ❌ Aspirational patterns not yet implemented

### Making Informed Decisions

When patterns are unclear:
1. **Search for multiple examples**: Use `grep_search` to find pattern usage
2. **Check feature analyses**: Look for patterns documented there
3. **Prioritize consistency**: Follow the most common pattern
4. **Document uncertainty**: Note when patterns are inferred

---

## Tool Usage

### Primary Tools

- **`read_file`**: Read analysis files and source code
- **`list_dir`**: Discover project structure
- **`grep_search`**: Find pattern usage across codebase
- **`semantic_search`**: Discover related code and conventions
- **`create_file`**: Write constitution file
- **`replace_string_in_file`**: Update existing constitution
- **`todo`**: Track progress through phases

### Workflow Pattern

```
1. Read docs/analyze.md → Understand project overview
2. Explore project structure → Document organization
3. Analyze feature analyses → Extract common patterns
4. Deep dive into source → Confirm patterns with examples
5. Write .github/constitution.md → Document findings
6. Validate → Ensure accuracy and completeness
7. Report → Summarize discoveries
```

---

## Example Invocations

### Generate New Constitution

```markdown
@excavator.constitution

Generate a constitution for this project based on the analysis
```

### Update Existing Constitution

```markdown
@excavator.constitution

Update the constitution with newly discovered patterns from recent features
```

### Focus on Specific Section

```markdown
@excavator.constitution

Update only the Security & Authorization section based on latest analysis
```

---

## Updating the Constitution

The constitution is a **living document**. Update it when:

- New architectural patterns are introduced
- Coding conventions evolve
- New feature categories are added
- Security patterns change
- Documentation standards are updated

Always base updates on actual code analysis, not assumptions.

<!--
Sync Impact Report
- Version change: 0.0.0 -> 1.0.0
- Modified principles: Replaced all template placeholders with project-specific principles for ContosoDashboard.
- Added sections: Additional Constraints, Development Workflow
- Removed sections: Placeholder template content and unresolved bracket tokens.
- Follow-up TODOs: None
-->

# ContosoDashboard Constitution

## Core Principles

### I. Training-First Alignment
ContosoDashboard is a training and demonstration application. Every change MUST preserve the project’s learning-oriented purpose, remain understandable to developers studying the codebase, and avoid introducing production-only assumptions that would hide the sample architecture.

Rationale: This repository is explicitly designed for education, so feature work should reinforce the patterns in the app rather than optimize for a production deployment.

### II. Security by Default
All new or modified features MUST maintain the existing mock authentication model, enforce authorization checks at both the page and service boundary, and prevent unauthorized access to user-specific data, projects, tasks, and notifications.

Rationale: The application demonstrates security concepts such as claims-based access, role policies, and user isolation, so safeguards must remain visible and testable in every iteration.

### III. Quality and Verification
Every feature change MUST be accompanied by clear acceptance criteria, a verifiable validation path, and evidence that the relevant build or test flow still works before the change is considered complete.

Rationale: A training application is most valuable when its examples are reliable, repeatable, and easy to validate by both learners and reviewers.

### IV. Change Isolation and Review
Features MUST be scoped to a bounded user need, documented in a way that explains the business value, and reviewed for regressions in authentication, data access, UI behavior, and project structure.

Rationale: Isolated changes make the Spec Kit workflow easier to follow and reduce the risk of accidental breakage in the demo application.

### V. Simplicity and Maintainability
The codebase MUST favor straightforward Blazor Server, Razor, and EF Core patterns over unnecessary abstraction or complexity. New code SHOULD remain easy to read, easy to trace, and easy to extend for learners.

Rationale: ContosoDashboard is intended as a readable reference implementation, so maintainability and clarity are as important as functionality.

## Additional Constraints

- The application MUST target .NET 10 for the primary build configuration unless a broader framework change is explicitly approved.
- The application MUST continue to use ASP.NET Core, Blazor Server, and EF Core patterns that are already present in the repository.
- Local-only development remains the default baseline; new features SHOULD avoid introducing external cloud dependencies unless the feature explicitly requires them for demonstration.
- Mock authentication and demo user data MUST remain suitable for offline training scenarios and MUST not imply production-ready identity management.
- Data handling MUST preserve user isolation, avoid unauthorized cross-user access, and remain consistent with the existing model and service layer design.

## Development Workflow

- Feature work MUST begin with a well-defined specification, plan, and task breakdown before implementation begins.
- Changes MUST follow the repository’s Spec Kit workflow, including review gates for specifications, plans, and task readiness.
- Implementation work MUST keep documentation aligned with the delivered behavior, especially when adding pages, services, models, or security-related logic.
- Before completion, the relevant build or verification command MUST run successfully and any remaining warnings or constraints MUST be documented.
- Contributions MUST keep the application’s existing training intent intact and MUST not silently degrade the mock authentication, authorization, or user-isolation behavior.

## Governance

This Constitution governs how ContosoDashboard evolves. It supersedes ad hoc conventions that conflict with the rules below.

- Amendments MUST be documented with a clear rationale, the affected principles or sections, and the corresponding version update.
- Versioning MUST follow semantic versioning: MAJOR for backward-incompatible governance changes, MINOR for new principles or materially expanded guidance, and PATCH for clarifications or non-semantic wording updates.
- All proposed changes MUST be reviewed for consistency with the project’s training purpose, security posture, and buildability.
- Compliance review MUST confirm that security-sensitive changes, authentication changes, data-access changes, and UI changes still preserve user isolation and the documented behavior.
- Any material deviation from this Constitution MUST be explicit in the change record and explained in the relevant feature documentation or review notes.

**Version**: 1.0.0 | **Ratified**: 2026-09-12 | **Last Amended**: 2026-09-12

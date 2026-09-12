# Research: Document Upload and Management

## Decision: Use local filesystem storage for the MVP, with a storage abstraction for future Azure Blob Storage migration

**Decision**: The initial implementation will store uploaded files on the local filesystem in a dedicated upload directory outside `wwwroot`, using an `IFileStorageService` abstraction with a `LocalFileStorageService` implementation. A future `AzureBlobStorageService` can replace the implementation through dependency injection without changing document business logic.

**Rationale**: This matches the repository’s offline-first training purpose, keeps the feature testable in a local environment, and aligns with the existing architecture guidance already described in the stakeholder document. It also satisfies the clarified requirement that the MVP remain local-first while preserving a clear migration path.

**Alternatives considered**:
- Direct Azure Blob Storage in the MVP: rejected because it would introduce cloud-only dependencies and make the training app harder to run offline.
- Saving files under `wwwroot`: rejected because it would expose user uploads directly and complicate authorization boundaries.
- Database-only storage for files: rejected because it is not a good fit for binary document payloads and would make later cloud migration harder.

## Decision: Add dedicated document entities and service layer rather than embedding document logic in existing pages

**Decision**: Introduce `Document`, `DocumentShare`, and `AuditEvent` entities, plus a `DocumentService` and storage abstraction, while keeping page-level logic thin and routing document operations through services.

**Rationale**: The current codebase already separates business work into services and EF Core models, so document functionality should follow that pattern for consistency, authorization, and testability.

**Alternatives considered**:
- Putting upload and metadata logic directly in Razor pages: rejected because it would bypass service-layer authorization and make reuse harder.
- Reusing task/project services for all document logic: rejected because it would blur responsibilities and reduce clarity.

## Decision: Use existing authentication and role policies, then enforce service-level permission checks

**Decision**: Continue using the existing mock authentication and role-based policies (`Employee`, `TeamLead`, `ProjectManager`, `Administrator`) and supplement them with document-specific authorization rules inside the service layer.

**Rationale**: The app already has an authorization model, so the document feature should extend that model instead of introducing a new identity system. Service-level checks also help prevent IDOR issues when pages are not fully constrained.

**Alternatives considered**:
- Relying only on UI-level access controls: rejected because it does not protect against direct service or URL-based misuse.
- Creating a separate document permission model from scratch: rejected because it would add unnecessary complexity for the training context.

## Decision: Treat search, preview, and recent-document integrations as core user journeys

**Decision**: The feature will include a searchable My Documents view, a Project Documents view, document preview for PDF and images, and dashboard/task integration as part of the initial MVP scope.

**Rationale**: These capabilities are central to the business value and already appear in the stakeholder requirements. They also make the feature demonstrably useful to real users rather than a storage-only placeholder.

**Alternatives considered**:
- Delivering upload-only support first with later search and preview: rejected because it would not satisfy the primary user need for finding and reusing documents efficiently.
- Omitting task/dashboard integration from the MVP: rejected because the stakeholder document explicitly states that integration is a core capability.

## Decision: Handle audit logging through a dedicated audit event model and service hooks

**Decision**: Uploads, downloads, previews, replacements, deletions, and sharing actions will be recorded in an `AuditEvent` model and surfaced to administrators through reporting support.

**Rationale**: This satisfies the audit requirement with a simple data model while preserving compatibility with the existing architecture.

**Alternatives considered**:
- Logging directly to application logs only: rejected because it would not support structured reporting or administrative review.
- Storing audit log entries in a separate external system: rejected because the current training app is intentionally offline and local-first.

## Decision: Keep scope bounded to the documented out-of-scope exclusions

**Decision**: The MVP will explicitly exclude version history, soft-delete/trash, collaborative editing, external integrations, and mobile apps.

**Rationale**: These items are explicitly out of scope and should remain out of scope to keep the feature realistic and implementable within the 8–10 week training timeline.

**Alternatives considered**:
- Including version history and collaborative editing in the first release: rejected because it would materially increase complexity and slow down the delivery timeline.
- Treating these items as optional later work: rejected because the current spec already clearly defines them as out of scope.

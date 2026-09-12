# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the repository structure and create the document feature scaffolding needed by all stories.

- [ ] T001 Create or confirm the document feature folders and file layout described in `specs/001-document-upload-management/plan.md`
- [ ] T002 [P] Add the document feature source files scaffold in `ContosoDashboard/Models/Document.cs`, `ContosoDashboard/Models/DocumentShare.cs`, `ContosoDashboard/Models/AuditEvent.cs`, `ContosoDashboard/Services/DocumentService.cs`, `ContosoDashboard/Services/IDocumentService.cs`, `ContosoDashboard/Services/IFileStorageService.cs`, `ContosoDashboard/Services/LocalFileStorageService.cs`, `ContosoDashboard/Pages/Documents.razor`, `ContosoDashboard/Pages/ProjectDocuments.razor`, and `ContosoDashboard/Pages/TaskDocuments.razor`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add the document persistence layer, storage abstraction, and service wiring so all user stories can build on the same foundation.

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

- [ ] T003 [P] Add `Document`, `DocumentShare`, and `AuditEvent` entities plus navigation properties and data annotations in `ContosoDashboard/Models/Document.cs`, `ContosoDashboard/Models/DocumentShare.cs`, and `ContosoDashboard/Models/AuditEvent.cs`
- [ ] T004 [P] Extend `ContosoDashboard/Data/ApplicationDbContext.cs` with `DbSet` properties, relationship configuration, indexes, and seed data for the new document entities
- [ ] T005 [P] Add the file storage abstraction and local filesystem implementation in `ContosoDashboard/Services/IFileStorageService.cs` and `ContosoDashboard/Services/LocalFileStorageService.cs`, including GUID-based stored file names and delete/download support
- [ ] T006 [P] Register the document service, storage implementation, and upload configuration in `ContosoDashboard/Program.cs` and `ContosoDashboard/appsettings.json`
- [ ] T007 [P] Implement the document business contract request/response model layer in `ContosoDashboard/Services/IDocumentService.cs` and the service shell in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T008 Implement document-specific authorization helpers and audit event creation paths in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T009 Add upload directory initialization and file-size/type validation plumbing in `ContosoDashboard/Services/LocalFileStorageService.cs` and `ContosoDashboard/Services/DocumentService.cs`

---

## Phase 3: User Story 1 - Upload and organize work documents (Priority: P1) 🎯 MVP

**Goal**: Allow authenticated employees to upload supported documents, add metadata, associate them with personal or project contexts, and view them in organized lists.

**Independent Test**: A user can upload multiple supported files, enter required metadata, and verify the documents appear in both the My Documents and project-specific document views.

### Implementation for User Story 1

- [ ] T010 [P] [US1] Implement the upload form, drag-and-drop/file picker UI, and success/error messaging in `ContosoDashboard/Pages/Documents.razor`
- [ ] T011 [P] [US1] Add the My Documents page list, category/project/date filters, and document cards in `ContosoDashboard/Pages/Documents.razor`
- [ ] T012 [US1] Implement `UploadDocumentAsync`, validation, file persistence, metadata persistence, and related audit entries in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T013 [US1] Implement project-aware document retrieval and filtering for authorized users in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T014 [P] [US1] Add the Project Documents page, project-scoped listing, and project membership checks in `ContosoDashboard/Pages/ProjectDocuments.razor`
- [ ] T015 [P] [US1] Add a navigation entry for Documents and project document links in `ContosoDashboard/Shared/NavMenu.razor`
- [ ] T016 [US1] Add document category, title, project, and tag validation rules plus clear rejection messaging in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Pages/Documents.razor`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - Find and browse documents quickly (Priority: P1)

**Goal**: Enable efficient search and browsing across person-level and project-level documents while restricting results to authorized users.

**Independent Test**: A user can search by title, description, tags, uploader, or project and only receive documents they are allowed to view.

### Implementation for User Story 2

- [ ] T017 [P] [US2] Add document search input, filter controls, and result rendering to `ContosoDashboard/Pages/Documents.razor`
- [ ] T018 [US2] Implement `SearchDocumentsAsync` and permission-aware query filtering in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T019 [US2] Add sorting, pagination or bounded result handling, and view state updates in `ContosoDashboard/Pages/Documents.razor`
- [ ] T020 [P] [US2] Extend the Project Documents view with searchable project document results and authorized visibility checks in `ContosoDashboard/Pages/ProjectDocuments.razor`
- [ ] T021 [US2] Ensure search and browse operations use `DocumentQueryOptions` and return only rows matching the current user’s access scope in `ContosoDashboard/Services/DocumentService.cs`

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently.

---

## Phase 5: User Story 3 - Share, preview, and manage uploaded documents (Priority: P2)

**Goal**: Let document owners share documents, preview supported file types, replace content, and delete or update metadata as authorized.

**Independent Test**: A document owner can share a file with a teammate, preview a PDF or image, and replace or delete the document if authorized.

### Implementation for User Story 3

- [ ] T022 [P] [US3] Add the document detail/actions panel, share controls, preview/download actions, and replace/delete buttons in `ContosoDashboard/Pages/Documents.razor` and `ContosoDashboard/Pages/ProjectDocuments.razor`
- [ ] T023 [US3] Implement `ShareDocumentAsync`, recipient validation, notification creation, and duplicate-share protection in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T024 [US3] Implement `UpdateDocumentMetadataAsync`, `ReplaceDocumentAsync`, and `DeleteDocumentAsync` with atomic update behavior and audit logging in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T025 [US3] Add preview/download handlers for supported content types in `ContosoDashboard/Pages/Documents.razor` and `ContosoDashboard/Services/DocumentService.cs`
- [ ] T026 [P] [US3] Update the document card layouts and status messaging for share, replace, and delete outcomes in `ContosoDashboard/Pages/Documents.razor`, `ContosoDashboard/Pages/ProjectDocuments.razor`, and `ContosoDashboard/wwwroot/css/site.css`

**Checkpoint**: At this point, User Story 3 should be independently functional.

---

## Phase 6: User Story 4 - Integrate document management with existing dashboard workflows (Priority: P2)

**Goal**: Surface document activity in the existing dashboard and task flows so users can find and attach relevant documents without leaving their normal workflow.

**Independent Test**: A user can access recent documents from the dashboard and attach a relevant document to a task.

### Implementation for User Story 4

- [ ] T027 [P] [US4] Extend the dashboard summary and recent document widget in `ContosoDashboard/Pages/Index.razor` and `ContosoDashboard/Services/DashboardService.cs`
- [ ] T028 [US4] Add a Recent Documents model/response shape to `ContosoDashboard/Services/DashboardService.cs` and surface it on the dashboard page
- [ ] T029 [P] [US4] Add task-level document attachment UI and document selection flow in `ContosoDashboard/Pages/Tasks.razor` and `ContosoDashboard/Pages/TaskDocuments.razor`
- [ ] T030 [US4] Implement task/document association logic, authorization checks, and audit logging in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Services/TaskService.cs`
- [ ] T031 [P] [US4] Wire in push-style notification support for new document activity in `ContosoDashboard/Services/NotificationService.cs` and `ContosoDashboard/Pages/Index.razor`

**Checkpoint**: At this point, User Story 4 should be independently functional.

---

## Phase 7: User Story 5 - Monitor compliance and audit document activity (Priority: P3)

**Goal**: Give administrators a usable audit and reporting surface for all document-related events.

**Independent Test**: An administrator can review audit logs and generate reports about document usage and activity.

### Implementation for User Story 5

- [ ] T032 [P] [US5] Add an administrator audit/reporting page or section in `ContosoDashboard/Pages/ProjectDocuments.razor` or a new `ContosoDashboard/Pages/Audit.razor` page
- [ ] T033 [US5] Implement `GetAuditEventsAsync` and reporting queries in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T034 [US5] Add admin-only access controls and role enforcement for audit/report retrieval in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T035 [P] [US5] Surface audit summaries, popular document types, and active uploader trends in the audit/reporting UI and `ContosoDashboard/wwwroot/css/site.css`

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Finish shared improvements that affect multiple user stories, including security hardening, page polish, and quickstart validation.

- [ ] T036 [P] Review and tighten security handling across `ContosoDashboard/Services/DocumentService.cs`, `ContosoDashboard/Services/LocalFileStorageService.cs`, and `ContosoDashboard/Program.cs` for authorization, file-path safety, and validation hardening
- [ ] T037 [P] Update `ContosoDashboard/wwwroot/css/site.css` to improve document cards, filter layouts, error banners, and responsive page behavior
- [ ] T038 [P] Update application navigation and landing-page guidance in `ContosoDashboard/Shared/NavMenu.razor` and `ContosoDashboard/Pages/Index.razor`
- [ ] T039 Run the quickstart validation scenarios from `specs/001-document-upload-management/quickstart.md` against the local application and document any follow-up fixes
- [ ] T040 Run `dotnet build` for `ContosoDashboard/ContosoDashboard.csproj` and resolve any compile errors or warnings introduced by the feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phase 3+)**: All depend on Foundational completion
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational; no dependencies on other stories
- **User Story 2 (P1)**: Depends on Foundation and should integrate with US1 document retrieval/search data shapes, but should remain independently testable
- **User Story 3 (P2)**: Depends on Foundation and UI structures from US1, but can be implemented independently once upload and list flows are available
- **User Story 4 (P2)**: Depends on US1 data structures and the task/dashboard integration points
- **User Story 5 (P3)**: Depends on the document service and audit logging introduced in the foundational layer

### Parallel Opportunities

- All tasks marked `[P]` can run in parallel where different files are involved
- User Story 1 can begin after Phase 2 and can be split across page UI, service logic, and navigation tasks
- User Story 2 can run in parallel with the remaining UI and service refinements for Story 1 once the shared document query layer exists
- User Story 3 can proceed in parallel with Story 2 after the core document service contract is in place
- Story 4 and Story 5 can be developed concurrently once the base document workflow is stable

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Stop and validate the upload, metadata, and listing flows independently

### Incremental Delivery

1. Complete Setup + Foundational → foundation ready
2. Ship User Story 1 → validate upload and organization
3. Ship User Story 2 → validate search and browsing
4. Ship User Story 3 → validate sharing, preview, and management
5. Ship User Story 4 → validate dashboard/task integration
6. Ship User Story 5 → validate audit/reporting

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Story 4 and Story 5 can then be delivered in parallel once the base document service and shared UI structures are stable

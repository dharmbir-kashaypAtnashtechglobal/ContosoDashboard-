# Quickstart Validation Guide

## Prerequisites

- .NET 10 SDK installed
- SQL Server LocalDB available for the current application
- The ContosoDashboard application running locally
- At least one authenticated user available through the existing mock login flow

## Validation Scenarios

### 1. Upload a valid document

1. Log in to the application using one of the existing mock users.
2. Open the Documents area from the main navigation.
3. Choose one or more supported files, enter a title, category, optional project, and optional tags, then submit the upload.
4. Confirm the upload completes successfully and the document appears in the My Documents view.

**Expected outcome**: The document is stored, metadata is visible, and the document count updates as expected.

### 2. Validate rejection for invalid uploads

1. Attempt to upload an oversized file or an unsupported file type.
2. Submit the upload form.
3. Check the validation message and the resulting document list.

**Expected outcome**: The upload is rejected, no document is persisted, and the user receives a clear error message.

### 3. Search and filter documents

1. Upload several documents with different titles, descriptions, tags, and project associations.
2. Use the search box and filters from the My Documents view.
3. Confirm that results update within the expected threshold and only include documents the user is authorized to access.

**Expected outcome**: Search results are accurate, filtered, and permission-aware.

### 4. Preview and download an authorized document

1. Open a previewable document such as a PDF or image.
2. Use the preview action and then the download action.
3. Confirm the browser preview loads correctly and the file downloads without exposing unauthorized content.

**Expected outcome**: The preview and download operations work for authorized users and fail appropriately for unauthorized users.

### 5. Share a document with another user

1. From a document card or detail view, share a document with a teammate.
2. Log in as the receiving user.
3. Open the shared documents section and confirm the document is visible.

**Expected outcome**: The recipient receives a notification and can access the shared document according to the defined permissions.

### 6. Validate dashboard and task integration

1. Navigate to the dashboard and find the Recent Documents widget.
2. Open a task detail page and attach a document relevant to that task.
3. Confirm the corresponding project and task relationships are reflected in document lists.

**Expected outcome**: The dashboard and task views surface document activity and related document associations correctly.

### 7. Validate audit reporting

1. Perform uploads, downloads, deletions, and share operations.
2. Open the audit or reporting area as an administrator.
3. Review the logged events and usage summaries.

**Expected outcome**: Administrative users can review activity records and summarize usage patterns.

## Expected Outcome Summary

The feature should demonstrate that document upload, classification, search, sharing, preview, task integration, and audits all work within the current ContosoDashboard experience while preserving security and offline‑training constraints.

# Document Service Contract

## Purpose

This contract defines the internal service and storage interfaces for the document management feature so the feature can be implemented consistently across pages, services, and future storage providers.

## Storage Contract

### IFileStorageService

```csharp
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string relativePath);
    Task DeleteAsync(string filePath);
    Task<Stream> DownloadAsync(string filePath);
    Task<string> GetPublicUrlAsync(string filePath, TimeSpan expiration);
}
```

### LocalFileStorageService

**Responsibilities**:
- Save uploaded files to a dedicated local directory outside `wwwroot`
- Write files using GUID-based names to avoid collisions and path traversal issues
- Delete files when a document is removed by an authorized user
- Support the same contract for future Azure Blob Storage replacement

### AzureBlobStorageService (future implementation)

**Responsibilities**:
- Implement the same `IFileStorageService` contract
- Replace only the storage provider, leaving business logic and page contracts unchanged

## Business Service Contract

### IDocumentService

```csharp
public interface IDocumentService
{
    Task<List<Document>> GetMyDocumentsAsync(int userId, DocumentQueryOptions options);
    Task<List<Document>> GetProjectDocumentsAsync(int userId, int projectId, DocumentQueryOptions options);
    Task<List<Document>> SearchDocumentsAsync(int userId, string query, DocumentQueryOptions options);
    Task<Document> UploadDocumentAsync(int userId, UploadDocumentRequest request);
    Task<bool> UpdateDocumentMetadataAsync(int userId, int documentId, UpdateDocumentRequest request);
    Task<bool> ReplaceDocumentAsync(int userId, int documentId, ReplaceDocumentRequest request);
    Task<bool> DeleteDocumentAsync(int userId, int documentId);
    Task<bool> ShareDocumentAsync(int userId, int documentId, ShareDocumentRequest request);
    Task<List<Document>> GetRecentDocumentsAsync(int userId, int count);
    Task<List<AuditEvent>> GetAuditEventsAsync(int userId, AuditQueryOptions options);
}
```

## Request and Response Contracts

### UploadDocumentRequest

- `Title`: required string
- `Description`: optional string
- `Category`: required string category value
- `ProjectId`: optional integer
- `TaskId`: optional integer
- `Tags`: optional collection of strings
- `File`: uploaded file stream and metadata

### UpdateDocumentRequest

- `Title`: optional string
- `Description`: optional string
- `Category`: optional string category value
- `Tags`: optional collection of strings

### ReplaceDocumentRequest

- `File`: replacement file stream
- `ContentType`: content type of replacement file

### ShareDocumentRequest

- `SharedWithUserIds`: collection of recipients
- `Message`: optional share note

## Authorization Expectations

- Only authenticated users can call document operations.
- `UploadDocumentAsync` must validate that the user can upload to the target project or to personal documents.
- `GetProjectDocumentsAsync` must enforce project membership and role-aware visibility rules.
- `DeleteDocumentAsync` must allow document owners and authorized project managers/administrators to remove documents.
- `ShareDocumentAsync` must validate that the sender is authorized to share the document and that the recipient is permitted to access it.
- `GetAuditEventsAsync` must be restricted to administrators or other authorized reporting roles.

## Operational Notes

- Document uploads should use GUID-based stored names and should never trust user-supplied file names for storage paths.
- Metadata updates and file replacement operations should be atomic from the perspective of the document record.
- Audit events should be created for all document actions that are relevant to security and reporting.
- The contract intentionally excludes version history, soft delete, and collaborative editing because these are out of scope for this release.

using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<List<Document>> GetUserDocumentsAsync(int userId, string? searchText = null, DocumentCategory? category = null, int? projectId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Document>> SearchDocumentsAsync(int userId, DocumentQueryOptions? options = null);
    Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId);
    Task<List<Document>> SearchProjectDocumentsAsync(int projectId, int requestingUserId, DocumentQueryOptions? options = null);
    Task<List<Document>> GetTaskDocumentsAsync(int taskId, int requestingUserId);
    Task<List<Document>> GetRecentDocumentsAsync(int userId, int maxResults = 5);
    Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId);
    Task<Document> UploadDocumentAsync(DocumentUploadRequest request, int uploaderId);
    Task<bool> UpdateDocumentMetadataAsync(int documentId, int requestingUserId, string title, string? description, string? tags, DocumentCategory? category, int? projectId);
    Task<Document?> ReplaceDocumentAsync(int documentId, int requestingUserId, DocumentUploadRequest request);
    Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId);
    Task<bool> ShareDocumentAsync(int documentId, int recipientUserId, int requestingUserId);
    Task<bool> AttachDocumentToTaskAsync(int taskId, int documentId, int requestingUserId);
    Task<List<AuditEvent>> GetAuditEventsAsync(int requestingUserId);
}

public class DocumentQueryOptions
{
    public string? SearchText { get; set; }
    public DocumentCategory? Category { get; set; }
    public int? ProjectId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "UploadedDate";
    public bool SortDescending { get; set; } = true;
    public int? MaxResults { get; set; } = 50;
}

public class DocumentUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; } = DocumentCategory.General;
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public int? ProjectId { get; set; }
    public string? OriginalFileName { get; set; }
    public Stream? FileContent { get; set; }
    public string? ContentType { get; set; }
}

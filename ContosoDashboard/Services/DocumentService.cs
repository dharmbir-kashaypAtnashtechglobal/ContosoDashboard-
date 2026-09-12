using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx",
        ".txt",
        ".png",
        ".jpg",
        ".jpeg",
        ".gif"
    };

    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;

    public DocumentService(ApplicationDbContext context, IFileStorageService fileStorageService, IUserService userService, INotificationService notificationService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _userService = userService;
        _notificationService = notificationService;
    }

    public async Task<List<Document>> GetUserDocumentsAsync(int userId, string? searchText = null, DocumentCategory? category = null, int? projectId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        return await SearchDocumentsAsync(userId, new DocumentQueryOptions
        {
            SearchText = searchText,
            Category = category,
            ProjectId = projectId,
            FromDate = fromDate,
            ToDate = toDate
        });
    }

    public async Task<List<Document>> SearchDocumentsAsync(int userId, DocumentQueryOptions? options = null)
    {
        options ??= new DocumentQueryOptions();

        var query = _context.Documents
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .Where(d => d.UploaderId == userId || d.Shares.Any(s => s.UserId == userId));

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            var normalizedSearch = options.SearchText.Trim();
            query = query.Where(d =>
                d.Title.Contains(normalizedSearch) ||
                (d.Description != null && d.Description.Contains(normalizedSearch)) ||
                (d.Tags != null && d.Tags.Contains(normalizedSearch)) ||
                d.Uploader.DisplayName.Contains(normalizedSearch) ||
                (d.Project != null && d.Project.Name.Contains(normalizedSearch)));
        }

        if (options.Category.HasValue)
        {
            query = query.Where(d => d.Category == options.Category.Value);
        }

        if (options.ProjectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == options.ProjectId.Value);
        }

        if (options.FromDate.HasValue)
        {
            query = query.Where(d => d.UploadedDate >= options.FromDate.Value);
        }

        if (options.ToDate.HasValue)
        {
            query = query.Where(d => d.UploadedDate <= options.ToDate.Value);
        }

        query = ApplySorting(query, options);

        var maxResults = options.MaxResults ?? 50;
        return await query
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId)
    {
        return await SearchProjectDocumentsAsync(projectId, requestingUserId);
    }

    public async Task<List<Document>> SearchProjectDocumentsAsync(int projectId, int requestingUserId, DocumentQueryOptions? options = null)
    {
        options ??= new DocumentQueryOptions();

        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null)
        {
            return new List<Document>();
        }

        var isProjectManager = project.ProjectManagerId == requestingUserId;
        var isProjectMember = project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);

        if (!isProjectManager && !isProjectMember)
        {
            return new List<Document>();
        }

        var query = _context.Documents
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .Where(d => d.ProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            var normalizedSearch = options.SearchText.Trim();
            query = query.Where(d =>
                d.Title.Contains(normalizedSearch) ||
                (d.Description != null && d.Description.Contains(normalizedSearch)) ||
                (d.Tags != null && d.Tags.Contains(normalizedSearch)) ||
                d.Uploader.DisplayName.Contains(normalizedSearch));
        }

        if (options.Category.HasValue)
        {
            query = query.Where(d => d.Category == options.Category.Value);
        }

        if (options.FromDate.HasValue)
        {
            query = query.Where(d => d.UploadedDate >= options.FromDate.Value);
        }

        if (options.ToDate.HasValue)
        {
            query = query.Where(d => d.UploadedDate <= options.ToDate.Value);
        }

        query = ApplySorting(query, options);

        var maxResults = options.MaxResults ?? 50;
        return await query
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<List<Document>> GetTaskDocumentsAsync(int taskId, int requestingUserId)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        if (task == null)
        {
            return new List<Document>();
        }

        var isAuthorized = await IsTaskAccessibleAsync(task, requestingUserId);
        if (!isAuthorized)
        {
            return new List<Document>();
        }

        return await _context.Documents
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .Where(d => d.TaskId == taskId)
            .OrderByDescending(d => d.UploadedDate)
            .Take(50)
            .ToListAsync();
    }

    public async Task<List<Document>> GetRecentDocumentsAsync(int userId, int maxResults = 5)
    {
        var query = _context.Documents
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .Where(d =>
                d.UploaderId == userId ||
                d.Shares.Any(s => s.UserId == userId) ||
                (d.ProjectId != null &&
                 (d.Project != null &&
                  (d.Project.ProjectManagerId == userId || d.Project.ProjectMembers.Any(pm => pm.UserId == userId)))));

        return await query
            .OrderByDescending(d => d.UploadedDate)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(d => d.Shares)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            return null;
        }

        if (!await CanAccessDocumentAsync(document, requestingUserId))
        {
            return null;
        }

        return document;
    }

    public async Task<Document> UploadDocumentAsync(DocumentUploadRequest request, int uploaderId)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.FileContent == null || request.FileContent.Length == 0)
        {
            throw new InvalidOperationException("Please select a file to upload.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Document title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
        {
            throw new InvalidOperationException("Original file name is required.");
        }

        if (request.FileContent.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("The selected file exceeds the 25 MB upload limit.");
        }

        var extension = Path.GetExtension(request.OriginalFileName);
        if (!SupportedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type. Please upload a PDF, Office document, text file, or image.");
        }

        if (request.ProjectId.HasValue)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId.Value);

            if (project == null)
            {
                throw new InvalidOperationException("Selected project was not found.");
            }

            var isProjectMember = project.ProjectMembers.Any(pm => pm.UserId == uploaderId) || project.ProjectManagerId == uploaderId;
            if (!isProjectMember)
            {
                throw new InvalidOperationException("You are not authorized to upload documents to that project.");
            }
        }

        var user = await _userService.GetUserByIdAsync(uploaderId);
        if (user == null)
        {
            throw new InvalidOperationException("Authenticated user could not be found.");
        }

        var storedFileName = await _fileStorageService.SaveAsync(request.FileContent, request.OriginalFileName, request.ContentType ?? "application/octet-stream");

        var document = new Document
        {
            Title = request.Title.Trim(),
            Category = request.Category,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim(),
            UploaderId = uploaderId,
            ProjectId = request.ProjectId,
            OriginalFileName = request.OriginalFileName.Trim(),
            StoredFileName = storedFileName,
            ContentType = request.ContentType ?? "application/octet-stream",
            FileSizeBytes = request.FileContent.Length,
            UploadedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        await CreateAuditEventAsync(document.DocumentId, uploaderId, "Upload", $"Uploaded document '{document.Title}'.");

        return document;
    }

    public async Task<bool> UpdateDocumentMetadataAsync(int documentId, int requestingUserId, string title, string? description, string? tags, DocumentCategory? category, int? projectId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Document title is required.");
        }

        var document = await _context.Documents
            .Include(d => d.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            return false;
        }

        if (!await CanModifyDocumentAsync(document, requestingUserId))
        {
            return false;
        }

        if (projectId.HasValue)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId.Value);

            if (project == null)
            {
                throw new InvalidOperationException("Selected project was not found.");
            }

            var isProjectMember = project.ProjectMembers.Any(pm => pm.UserId == requestingUserId) || project.ProjectManagerId == requestingUserId;
            if (!isProjectMember)
            {
                throw new InvalidOperationException("You are not authorized to move this document to that project.");
            }

            document.ProjectId = projectId.Value;
        }

        document.Title = title.Trim();
        document.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        document.Tags = string.IsNullOrWhiteSpace(tags) ? null : tags.Trim();
        document.Category = category ?? document.Category;
        document.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await CreateAuditEventAsync(documentId, requestingUserId, "Update", $"Updated metadata for document '{document.Title}'.");

        return true;
    }

    public async Task<Document?> ReplaceDocumentAsync(int documentId, int requestingUserId, DocumentUploadRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var document = await _context.Documents
            .Include(d => d.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            return null;
        }

        if (!await CanModifyDocumentAsync(document, requestingUserId))
        {
            return null;
        }

        if (request.FileContent == null || request.FileContent.Length == 0)
        {
            throw new InvalidOperationException("Please select a file to upload.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Document title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
        {
            throw new InvalidOperationException("Original file name is required.");
        }

        if (request.FileContent.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("The selected file exceeds the 25 MB upload limit.");
        }

        var extension = Path.GetExtension(request.OriginalFileName);
        if (!SupportedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type. Please upload a PDF, Office document, text file, or image.");
        }

        var oldStoredFileName = document.StoredFileName;
        var storedFileName = await _fileStorageService.SaveAsync(request.FileContent, request.OriginalFileName, request.ContentType ?? "application/octet-stream");

        document.Title = request.Title.Trim();
        document.Category = request.Category;
        document.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        document.Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim();
        document.OriginalFileName = request.OriginalFileName.Trim();
        document.StoredFileName = storedFileName;
        document.ContentType = request.ContentType ?? "application/octet-stream";
        document.FileSizeBytes = request.FileContent.Length;
        document.UpdatedDate = DateTime.UtcNow;

        if (request.ProjectId.HasValue)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId.Value);

            if (project == null)
            {
                throw new InvalidOperationException("Selected project was not found.");
            }

            document.ProjectId = request.ProjectId.Value;
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(oldStoredFileName) && !string.Equals(oldStoredFileName, storedFileName, StringComparison.Ordinal))
        {
            await _fileStorageService.DeleteAsync(oldStoredFileName);
        }

        await CreateAuditEventAsync(documentId, requestingUserId, "Replace", $"Replaced document '{document.Title}'.");

        return document;
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            return false;
        }

        if (!await CanModifyDocumentAsync(document, requestingUserId))
        {
            return false;
        }

        await CreateAuditEventAsync(documentId, requestingUserId, "Delete", $"Deleted document '{document.Title}'.");

        if (!string.IsNullOrWhiteSpace(document.StoredFileName))
        {
            await _fileStorageService.DeleteAsync(document.StoredFileName);
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ShareDocumentAsync(int documentId, int recipientUserId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null || !await CanModifyDocumentAsync(document, requestingUserId))
        {
            return false;
        }

        if (recipientUserId == requestingUserId)
        {
            return false;
        }

        var recipientUser = await _userService.GetUserByIdAsync(recipientUserId);
        if (recipientUser == null)
        {
            return false;
        }

        if (document.ProjectId.HasValue)
        {
            var isProjectMember = document.Project?.ProjectMembers.Any(pm => pm.UserId == recipientUserId) ?? false;
            var isProjectManager = document.Project?.ProjectManagerId == recipientUserId;
            if (!isProjectMember && !isProjectManager)
            {
                return false;
            }
        }

        var existingShare = await _context.DocumentShares
            .FirstOrDefaultAsync(s => s.DocumentId == documentId && s.UserId == recipientUserId);

        if (existingShare != null)
        {
            return false;
        }

        _context.DocumentShares.Add(new DocumentShare
        {
            DocumentId = documentId,
            UserId = recipientUserId,
            SharedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(new Notification
        {
            UserId = recipientUserId,
            Title = "Document Shared",
            Message = $"A document named '{document.Title}' was shared with you.",
            Type = NotificationType.ProjectUpdate,
            Priority = NotificationPriority.Important
        });

        await CreateAuditEventAsync(documentId, requestingUserId, "Share", $"Shared document '{document.Title}' with {recipientUser.DisplayName}.");

        return true;
    }

    public async Task<bool> AttachDocumentToTaskAsync(int taskId, int documentId, int requestingUserId)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        if (task == null)
        {
            return false;
        }

        var isAuthorized = await IsTaskAccessibleAsync(task, requestingUserId);
        if (!isAuthorized)
        {
            return false;
        }

        var document = await _context.Documents
            .Include(d => d.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            return false;
        }

        if (!await CanAccessDocumentAsync(document, requestingUserId))
        {
            return false;
        }

        document.TaskId = taskId;
        if (!document.ProjectId.HasValue && task.ProjectId.HasValue)
        {
            document.ProjectId = task.ProjectId.Value;
        }

        document.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await CreateAuditEventAsync(documentId, requestingUserId, "AttachToTask", $"Attached document '{document.Title}' to task '{task.Title}'.");

        return true;
    }

    public async Task<List<AuditEvent>> GetAuditEventsAsync(int requestingUserId)
    {
        var user = await _userService.GetUserByIdAsync(requestingUserId);
        if (user == null || user.Role != UserRole.Administrator)
        {
            return new List<AuditEvent>();
        }

        return await _context.AuditEvents
            .Include(e => e.Document)
            .Include(e => e.User)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();
    }

    private IQueryable<Document> ApplySorting(IQueryable<Document> query, DocumentQueryOptions options)
    {
        if (options.SortBy.Equals("Title", StringComparison.OrdinalIgnoreCase))
        {
            return options.SortDescending
                ? query.OrderByDescending(d => d.Title)
                : query.OrderBy(d => d.Title);
        }

        if (options.SortBy.Equals("Category", StringComparison.OrdinalIgnoreCase))
        {
            return options.SortDescending
                ? query.OrderByDescending(d => d.Category)
                : query.OrderBy(d => d.Category);
        }

        return options.SortDescending
            ? query.OrderByDescending(d => d.UploadedDate)
            : query.OrderBy(d => d.UploadedDate);
    }

    private async Task<bool> CanAccessDocumentAsync(Document document, int requestingUserId)
    {
        var isUploader = document.UploaderId == requestingUserId;
        var isProjectManager = document.Project != null && document.Project.ProjectManagerId == requestingUserId;
        var isProjectMember = document.Project != null && document.Project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);
        var isSharedWithUser = document.Shares.Any(s => s.UserId == requestingUserId);

        return isUploader || isProjectManager || isProjectMember || isSharedWithUser;
    }

    private async Task<bool> CanModifyDocumentAsync(Document document, int requestingUserId)
    {
        var project = document.Project;
        if (project == null && document.ProjectId.HasValue)
        {
            project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == document.ProjectId.Value);
        }

        var isUploader = document.UploaderId == requestingUserId;
        var isProjectManager = project != null && project.ProjectManagerId == requestingUserId;
        var isProjectMember = project != null && project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);

        return isUploader || isProjectManager || isProjectMember;
    }

    private async Task<bool> IsTaskAccessibleAsync(TaskItem task, int requestingUserId)
    {
        var isAssignedUser = task.AssignedUserId == requestingUserId;
        var isCreator = task.CreatedByUserId == requestingUserId;
        var isProjectMember = task.Project != null && task.Project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);
        var isProjectManager = task.Project != null && task.Project.ProjectManagerId == requestingUserId;

        return isAssignedUser || isCreator || isProjectMember || isProjectManager;
    }

    private async Task CreateAuditEventAsync(int documentId, int userId, string eventType, string details)
    {
        _context.AuditEvents.Add(new AuditEvent
        {
            DocumentId = documentId,
            UserId = userId,
            EventType = eventType,
            Details = details,
            EventDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}

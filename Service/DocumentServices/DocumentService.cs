using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DocumentDtos;
using GrantTrack.Repository.DocumentRepository;

namespace GrantTrack.Service.DocumentServices;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DocumentService> _logger;

    /// <summary>
    /// Upload root lives at <ContentRoot>/uploads/applications. We never
    /// expose this path to clients — they only ever see the guarded
    /// /api/v1/applications/{id}/documents/{docId}/download URL.
    /// </summary>
    private string UploadRoot => Path.Combine(_env.ContentRootPath, "uploads", "applications");

    private static readonly long MaxBytes = 10L * 1024 * 1024;          // 10 MB
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".txt",
    };

    public DocumentService(
        IDocumentRepository repo,
        IWebHostEnvironment env,
        ILogger<DocumentService> logger)
    {
        _repo = repo;
        _env = env;
        _logger = logger;
    }

    public async Task<DocumentResponseDto> UploadAsync(
        int applicationId, int currentUserId, IFormFile file, string? docType)
    {
        // 1. Sanity checks
        if (file is null || file.Length == 0)
            throw new ArgumentException("File is empty.");
        if (file.Length > MaxBytes)
            throw new ArgumentException($"File exceeds the {MaxBytes / (1024 * 1024)} MB limit.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
            throw new ArgumentException(
                $"File type '{ext}' is not allowed. Accepted: {string.Join(", ", AllowedExt)}.");

        // 2. Ownership: only the applicant who owns the application can upload.
        var ownerId = await _repo.GetApplicantIdForApplicationAsync(applicationId)
            ?? throw new KeyNotFoundException("Application not found.");
        if (ownerId != currentUserId)
            throw new UnauthorizedAccessException("You don't own this application.");

        // 3. Persist file to disk under a per-application folder.
        var appFolder = Path.Combine(UploadRoot, applicationId.ToString());
        Directory.CreateDirectory(appFolder);

        // GUID-prefixed filename keeps it unique on disk while preserving the
        // original name for display ({guid}_{originalFilename.ext}).
        var safeOriginal = Path.GetFileName(file.FileName);   // strip any path prefix
        var storedName = $"{Guid.NewGuid():N}_{safeOriginal}";
        var filePath = Path.Combine(appFolder, storedName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        // 4. Persist DB row. The schema has BOTH columns — set the typo'd
        // one explicitly; the repo sets the shadow FK ApplicationId.
        var doc = new Document
        {
            ApplicantionId = applicationId,
            // FileURI stores the path relative to UploadRoot so the service
            // can resolve it back to disk later: "{appId}/{storedName}".
            FileURI = Path.Combine(applicationId.ToString(), storedName),
            DocType = string.IsNullOrWhiteSpace(docType) ? "Other" : docType.Trim(),
        };

        var saved = await _repo.AddAsync(doc, applicationId);
        _logger.LogInformation("Document {Id} uploaded for application {AppId}", saved.DocumentId, applicationId);
        return Map(saved);
    }

    public async Task<List<DocumentResponseDto>> ListAsync(int applicationId, int currentUserId, bool privilegedReader)
    {
        if (!privilegedReader)
            await EnsureReadAccessAsync(applicationId, currentUserId);
        // else: caller passed the [Authorize(Roles=Approver|ComplianceOfficer)]
        // gate at the controller — no per-app check needed.
        var docs = await _repo.GetByApplicationIdAsync(applicationId);
        return docs.Select(Map).ToList();
    }

    public async Task DeleteAsync(int documentId, int currentUserId)
    {
        var doc = await _repo.GetByIdAsync(documentId)
            ?? throw new KeyNotFoundException("Document not found.");

        var appId = doc.ApplicantionId;
        var ownerId = await _repo.GetApplicantIdForApplicationAsync(appId)
            ?? throw new KeyNotFoundException("Application not found.");
        if (ownerId != currentUserId)
            throw new UnauthorizedAccessException("You don't own this document.");

        // Remove the file from disk first. If the DB delete then fails the
        // record will still point at a missing file — acceptable: the next
        // delete attempt succeeds and the orphan is just a stale row.
        var fullPath = Path.Combine(UploadRoot, doc.FileURI);
        try
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete file {Path}", fullPath);
        }

        await _repo.DeleteAsync(doc);
    }

    public async Task<(string filePath, string contentType, string fileName)> GetDownloadAsync(
        int documentId, int currentUserId, bool privilegedReader)
    {
        var doc = await _repo.GetByIdAsync(documentId)
            ?? throw new KeyNotFoundException("Document not found.");

        if (!privilegedReader)
            await EnsureReadAccessAsync(doc.ApplicantionId, currentUserId);

        var fullPath = Path.Combine(UploadRoot, doc.FileURI);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File missing on disk.", fullPath);

        var ext = Path.GetExtension(doc.FileURI);
        var contentType = MimeFor(ext);
        var fileName = ParseOriginalFileName(doc.FileURI);
        return (fullPath, contentType, fileName);
    }

    /// <summary>
    /// Read access is granted to:
    ///   • the applicant who owns the application, OR
    ///   • a Reviewer who has been assigned to the application (i.e. has a
    ///     row in Reviews matching applicationId + reviewerId).
    /// Anyone else gets a 403 even if their JWT role passes the controller
    /// guard — this is the second line of defence.
    /// </summary>
    private async Task EnsureReadAccessAsync(int applicationId, int currentUserId)
    {
        var ownerId = await _repo.GetApplicantIdForApplicationAsync(applicationId)
            ?? throw new KeyNotFoundException("Application not found.");

        if (ownerId == currentUserId) return;
        if (await _repo.IsReviewerAssignedAsync(applicationId, currentUserId)) return;

        throw new UnauthorizedAccessException(
            "You don't have access to this application's documents.");
    }

    // ------------------------ helpers ---------------------------------------

    private DocumentResponseDto Map(Document d) => new()
    {
        DocumentId = d.DocumentId,
        ApplicationId = d.ApplicantionId,
        DocType = d.DocType,
        FileName = ParseOriginalFileName(d.FileURI),
        DownloadUrl = $"/api/v1/applications/{d.ApplicantionId}/documents/{d.DocumentId}/download",
    };

    private static string ParseOriginalFileName(string fileUri)
    {
        // Stored as "{appId}/{guid}_{originalName}"
        var leaf = Path.GetFileName(fileUri);
        var underscore = leaf.IndexOf('_');
        return underscore >= 0 && underscore < leaf.Length - 1
            ? leaf[(underscore + 1)..]
            : leaf;
    }

    private static string MimeFor(string ext) => ext.ToLowerInvariant() switch
    {
        ".pdf"          => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png"          => "image/png",
        ".doc"          => "application/msword",
        ".docx"         => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls"          => "application/vnd.ms-excel",
        ".xlsx"         => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".txt"          => "text/plain",
        _               => "application/octet-stream",
    };
}

using System.Security.Cryptography;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DocumentDtos;
using GrantTrack.Repository.ApplicationRepositories;
using GrantTrack.Repository.DocumentRepositories;
using GrantTrack.Utility;

namespace GrantTrack.Service.DocumentServices;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepo;
    private readonly IApplicationRepository _applicationRepo;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public DocumentService(
        IDocumentRepository documentRepo,
        IApplicationRepository applicationRepo,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        _documentRepo = documentRepo;
        _applicationRepo = applicationRepo;
        _config = config;
        _env = env;
    }

    public async Task<UploadDocumentResponseDto> UploadAsync(
        int applicantId, UploadDocumentRequestDto dto)
    {
        var file = dto.File;

        //File must be present and non-empty
        if (file == null || file.Length == 0)
            throw new InvalidOperationException(Messages.EmptyFile);

        //Application must exist
        var application = await _applicationRepo.GetByIdAsync(dto.ApplicationId)
            ?? throw new KeyNotFoundException(Messages.ApplicationNotFound);

        // Only the owner can upload documents
        if (application.ApplicantId != applicantId)
            throw new UnauthorizedAccessException(Messages.Forbidden);

        //Application must not already be decided
        if (application.Status == ApplicationStatus.Approved ||
            application.Status == ApplicationStatus.Rejected)
            throw new InvalidOperationException(Messages.ApplicationAlreadyDecided);

        //File size must be within limit
        var maxSize = _config.GetValue<long>("DocumentUpload:MaxFileSizeBytes");
        if (file.Length > maxSize)
            throw new InvalidOperationException(Messages.FileSizeExceeded);

        //Content type must be in the allowed list
        var allowedTypes = _config.GetSection("DocumentUpload:AllowedContentTypes")
            .Get<string[]>() ?? Array.Empty<string>();
        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException(Messages.UnsupportedContentType);

        var safeFileName = SanitizeFileName(file.FileName);

        // Determine version — increment if same filename already uploaded
        var exists = await _documentRepo.ExistsForApplicationAsync(dto.ApplicationId, safeFileName);
        var version = exists ? 2 : 1;

        // Save file to wwwroot/uploads/{applicationId}/
        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadRoot = Path.Combine(webRootPath, "uploads", dto.ApplicationId.ToString());
        Directory.CreateDirectory(uploadRoot);

        // Version the filename if needed: e.g. contract_v2.pdf
        var versionedName = version > 1
            ? $"{Path.GetFileNameWithoutExtension(safeFileName)}_v{version}{Path.GetExtension(safeFileName)}"
            : safeFileName;

        var filePath = Path.Combine(uploadRoot, versionedName);

        string hash;
        await using (var inStream = file.OpenReadStream())
        using (var sha256 = SHA256.Create())
        await using (var fileOut = File.Create(filePath))
        await using (var cryptoStream = new CryptoStream(fileOut, sha256, CryptoStreamMode.Write))
        {
            await inStream.CopyToAsync(cryptoStream);
            await cryptoStream.FlushFinalBlockAsync();
            hash = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();
        }

        // Persist the document record (single insert with everything filled in)
        var document = new Document
        {
            ApplicationId = dto.ApplicationId,
            DocType = dto.DocType,
            FileName = safeFileName,
            FileURI = $"/uploads/{dto.ApplicationId}/{versionedName}",
            ContentType = file.ContentType,
            Hash = hash,
            Version = version,
            CreatedAt = DateTime.UtcNow,
            UploadedAt = DateTime.UtcNow,
        };

        var created = await _documentRepo.CreateAsync(document);

        return new UploadDocumentResponseDto
        {
            DocumentId = created.DocumentId,
            FileName = created.FileName,
            FileURI = created.FileURI,
            Hash = created.Hash,
            Version = created.Version,
            UploadedAt = created.UploadedAt!.Value,
        };
    }

    public async Task<DocumentDownloadDto> DownloadAsync(int userId, UserRole role, int documentId)
    {
        //Document must exist
        var document = await _documentRepo.GetByIdAsync(documentId)
            ?? throw new KeyNotFoundException(Messages.DocumentNotFound);

        //Authorization
        //   - Applicant: only their own documents
        //   - Admin / Reviewer: any document
        var isPrivileged = role == UserRole.Admin || role == UserRole.Reviewer;
        var isOwner = document.Application?.ApplicantId == userId;

        if (!isPrivileged && !isOwner)
            throw new UnauthorizedAccessException(Messages.Forbidden);

        // Document must have actually been uploaded (not just a DB row)
        if (string.IsNullOrEmpty(document.FileURI))
            throw new InvalidOperationException(Messages.DocumentNotUploaded);

        // Resolve absolute path on disk.
        // FileURI is something like "/uploads/5/contract.pdf"
        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var relativePath = document.FileURI.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(webRootPath, relativePath);

        // ensure the resolved path is still inside the upload root.
        // Protects against any future bug that lets a tampered FileURI escape the directory.
        var uploadRoot = Path.Combine(webRootPath, "uploads");
        var fullAbsolute = Path.GetFullPath(absolutePath);
        var fullUploadRoot = Path.GetFullPath(uploadRoot);
        if (!fullAbsolute.StartsWith(fullUploadRoot, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException(Messages.Forbidden);

        // File must actually exist on disk
        if (!File.Exists(fullAbsolute))
            throw new KeyNotFoundException(Messages.FileNotFound);

        // Open as a read-only async stream.
        // The controller will dispose it after streaming the response to the client.
        var stream = new FileStream(
            fullAbsolute,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        // Versioned filename for the download (matches what's actually on disk)
        var versionedName = document.Version > 1
            ? $"{Path.GetFileNameWithoutExtension(document.FileName)}_v{document.Version}{Path.GetExtension(document.FileName)}"
            : document.FileName;

        return new DocumentDownloadDto
        {
            Content = stream,
            FileName = versionedName,
            ContentType = string.IsNullOrWhiteSpace(document.ContentType)
                            ? "application/octet-stream"
                            : document.ContentType,
            Length = stream.Length,
        };
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = string.Concat(fileName.Split(invalid));
        return safe.Length > 0 ? safe : "document";
    }
}
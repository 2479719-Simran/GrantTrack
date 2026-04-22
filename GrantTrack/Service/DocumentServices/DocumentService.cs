using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DocumentDtos;
using GrantTrack.Repository.ApplicationRepositories;
using GrantTrack.Repository.DocumentRepositories;
using GrantTrack.Utility;
using Microsoft.IdentityModel.Tokens;

namespace GrantTrack.Service.DocumentServices;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepo;
    private readonly IApplicationRepository _applicationRepo;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    private const int UploadTokenExpiryMinutes = 15;

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

    public async Task<GenerateUploadUrlResponseDto> GenerateUploadUrlAsync(
        int applicantId, GenerateUploadUrlRequestDto dto)
    {
        var applicationId = dto.ApplicationId;

        // Guard 1: Application must exist
        var application = await _applicationRepo.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException(Messages.ApplicationNotFound);

        // Guard 2: Only the owner can upload documents
        if (application.ApplicantId != applicantId)
            throw new UnauthorizedAccessException(Messages.Forbidden);

        // Guard 3: Application must not already be decided
        if (application.Status == ApplicationStatus.Approved ||
            application.Status == ApplicationStatus.Rejected)
            throw new InvalidOperationException(Messages.ApplicationAlreadyDecided);

        var safeFileName = SanitizeFileName(dto.FileName);

        // Determine version — increment if same filename already uploaded
        var exists = await _documentRepo.ExistsForApplicationAsync(applicationId, safeFileName);
        var version = exists ? 2 : 1;

        // Create pending document record
        var document = new Document
        {
            ApplicationId = applicationId,
            DocType = dto.DocType,
            FileName = safeFileName,
            FileURI = string.Empty,
            Version = version,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await _documentRepo.CreateAsync(document);

        // Generate short-lived upload token
        var expiresAt = DateTime.UtcNow.AddMinutes(UploadTokenExpiryMinutes);
        var token = GenerateUploadToken(applicationId, created.DocumentId, expiresAt);

        // NOTE: Url is left empty here — the controller fills it in.
        return new GenerateUploadUrlResponseDto
        {
            Url = string.Empty,
            DocumentId = created.DocumentId,
            ExpiresAt = expiresAt,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type",   dto.ContentType },
                { "X-Document-Id",  created.DocumentId.ToString() },
                { "X-Upload-Token", token }
            }
        };
    }

    public async Task ConfirmUploadAsync(
        int applicationId, int documentId, string uploadToken, Stream fileStream)
    {
        // Validate upload token
        var claims = ValidateUploadToken(uploadToken);
        var tokenAppId = int.Parse(claims.FindFirstValue("applicationId")
            ?? throw new UnauthorizedAccessException(Messages.InvalidUploadToken));
        var tokenDocId = int.Parse(claims.FindFirstValue("documentId")
            ?? throw new UnauthorizedAccessException(Messages.InvalidUploadToken));

        // Cross-check: DTO values must match what was signed into the token
        if (tokenAppId != applicationId || tokenDocId != documentId)
            throw new UnauthorizedAccessException(Messages.InvalidUploadToken);

        // Fetch the document record
        var document = await _documentRepo.GetByIdAsync(documentId)
            ?? throw new KeyNotFoundException(Messages.DocumentNotFound);

        if (document.ApplicationId != applicationId)
            throw new UnauthorizedAccessException(Messages.Forbidden);

        // Guard: if the file was already uploaded with this document record, reject replay
        if (!string.IsNullOrEmpty(document.Hash))
            throw new InvalidOperationException(Messages.DocumentAlreadyUploaded);

        // Save file to wwwroot/uploads/{applicationId}/
        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadRoot  = Path.Combine(webRootPath, "uploads", applicationId.ToString());
        Directory.CreateDirectory(uploadRoot);

        // Version the filename if needed: e.g. contract_v2.pdf
        var versionedName = document.Version > 1
            ? $"{Path.GetFileNameWithoutExtension(document.FileName)}_v{document.Version}{Path.GetExtension(document.FileName)}"
            : document.FileName;

        var filePath = Path.Combine(uploadRoot, versionedName);

        // Write file and compute SHA-256 hash in a single pass
        string hash;
        using (var sha256 = SHA256.Create())
        using (var fileOut = File.Create(filePath))
        using (var cryptoStream = new CryptoStream(fileOut, sha256, CryptoStreamMode.Write))
        {
            await fileStream.CopyToAsync(cryptoStream);
            await cryptoStream.FlushFinalBlockAsync();
            hash = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();
        }

        // Update document with hash, URI and upload time
        document.FileURI = $"/uploads/{applicationId}/{versionedName}";
        document.Hash = hash;
        document.UploadedAt = DateTime.UtcNow;

        await _documentRepo.UpdateAsync(document);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------
    private string GenerateUploadToken(int applicationId, int documentId, DateTime expiresAt)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("applicationId", applicationId.ToString()),
            new Claim("documentId",    documentId.ToString()),
            new Claim("purpose",       "document-upload"),
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal ValidateUploadToken(string token)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedAccessException(Messages.InvalidUploadToken);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = string.Concat(fileName.Split(invalid));
        return safe.Length > 0 ? safe : "document";
    }
}
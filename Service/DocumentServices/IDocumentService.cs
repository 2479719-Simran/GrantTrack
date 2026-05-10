using GrantTrack.Dto.DocumentDtos;
using Microsoft.AspNetCore.Http;

namespace GrantTrack.Service.DocumentServices;

public interface IDocumentService
{
    Task<DocumentResponseDto> UploadAsync(int applicationId, int currentUserId, IFormFile file, string? docType);

    /// <summary>
    /// List documents for an application. <paramref name="privilegedReader"/>
    /// is true for roles that get blanket read access (Approver, Compliance);
    /// false for roles that need a per-application check (Applicant, Reviewer).
    /// </summary>
    Task<List<DocumentResponseDto>> ListAsync(int applicationId, int currentUserId, bool privilegedReader);

    Task DeleteAsync(int documentId, int currentUserId);

    /// <summary>
    /// Resolves and returns (filePath, contentType, fileName) for a download.
    /// See <see cref="ListAsync"/> for the privilegedReader semantics.
    /// </summary>
    Task<(string filePath, string contentType, string fileName)> GetDownloadAsync(int documentId, int currentUserId, bool privilegedReader);
}

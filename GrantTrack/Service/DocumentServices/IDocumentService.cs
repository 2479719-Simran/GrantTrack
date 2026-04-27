using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DocumentDtos;

namespace GrantTrack.Service.DocumentServices;

public interface IDocumentService
{
    Task<UploadDocumentResponseDto> UploadAsync(int applicantId, UploadDocumentRequestDto dto);
    Task<DocumentDownloadDto> DownloadAsync(int userId, UserRole role, int documentId);
}
using GrantTrack.Dto.DocumentDtos;

namespace GrantTrack.Service.DocumentServices;

public interface IDocumentService
{
    Task<GenerateUploadUrlResponseDto> GenerateUploadUrlAsync(
        int applicantId, GenerateUploadUrlRequestDto dto);

    Task ConfirmUploadAsync(
        int applicationId, int documentId, string uploadToken, Stream fileStream);
}
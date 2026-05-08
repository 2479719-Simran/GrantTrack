using System;

namespace GrantTrack.Service.RequiredDocumentServices;

using GrantTrack.Dto.RequiredDocumentsDtos;

public interface IRequiredDocumentService
{
    Task<CreateRequiredDocumentResponseDto> CreateDocument(CreateRequiredDocumentRequestDto request);
    Task<IEnumerable<GetRequiredDocumentResponseDto>> GetDocuments();
    Task<IEnumerable<GetRequiredDocumentResponseDto>> GetDocumentsByProgramId(int programId);
    Task<UpdateRequiredDocumentResponseDto> UpdateDocument(int documentId, UpdateRequiredDocumentRequestDto request);
    Task<DeleteRequiredDocumentResponseDto> DeleteDocument(int documentId);
}

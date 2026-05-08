
using GrantTrack.Dto.RequiredDocumentsDtos;

namespace GrantTrack.Repository.RequiredDocumentRepositories;

public interface IRequiredDocumentRepository
{
    Task<bool> ContainsDocumentId(int documentId);
    Task<bool> ContainsProgramId(int programId);
    Task<CreateRequiredDocumentResponseDto> CreateDocument(CreateRequiredDocumentRequestDto request);
    Task<IEnumerable<GetRequiredDocumentResponseDto>> GetDocuments();
    Task<IEnumerable<GetRequiredDocumentResponseDto>> GetDocumentsByProgramId(int programId);
    Task<UpdateRequiredDocumentResponseDto> UpdateDocument(int documentId, UpdateRequiredDocumentRequestDto request);
    Task<DeleteRequiredDocumentResponseDto> DeleteDocument(int documentId);
}



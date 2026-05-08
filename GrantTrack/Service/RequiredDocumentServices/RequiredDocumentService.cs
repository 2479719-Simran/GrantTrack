using GrantTrack.Dto.RequiredDocumentsDtos;
using GrantTrack.Repository.ProgramRepository;
using GrantTrack.Repository.RequiredDocumentRepositories;
 
namespace GrantTrack.Service.RequiredDocumentServices;
 
public class RequiredDocumentService : IRequiredDocumentService
{
    private readonly IRequiredDocumentRepository documentRepository;
    private readonly IProgramRepository programRepository;
 
    public RequiredDocumentService(
        IRequiredDocumentRepository documentRepository,
        IProgramRepository programRepository)
    {
        this.documentRepository = documentRepository;
        this.programRepository  = programRepository;
    }
 
    public async Task<CreateRequiredDocumentResponseDto> CreateDocument(CreateRequiredDocumentRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");
 
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Document name is required.");
 
        var programExists = await programRepository.ExistsAsync(request.ProgramId);
        if (!programExists)
            throw new KeyNotFoundException($"Program with ID {request.ProgramId} does not exist.");
 
        var programActive = await programRepository.IsActiveAsync(request.ProgramId);
        if (!programActive)
            throw new InvalidOperationException($"Program with ID {request.ProgramId} is inactive. Cannot add documents.");
 
        return await documentRepository.CreateDocument(request);
    }
 
    public async Task<IEnumerable<GetRequiredDocumentResponseDto>> GetDocuments()
    {
        return await documentRepository.GetDocuments();
    }
 
    public async Task<IEnumerable<GetRequiredDocumentResponseDto>> GetDocumentsByProgramId(int programId)
    {
        if (programId <= 0)
            throw new ArgumentException("Program ID must be greater than zero.");
 
        var programExists = await programRepository.ExistsAsync(programId);
        if (!programExists)
            throw new KeyNotFoundException($"Program with ID {programId} does not exist.");
 
        return await documentRepository.GetDocumentsByProgramId(programId);
    }
 
    public async Task<UpdateRequiredDocumentResponseDto> UpdateDocument(int documentId, UpdateRequiredDocumentRequestDto request)
    {
        if (documentId <= 0)
            throw new ArgumentException("Document ID must be greater than zero.");
 
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");
 
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Document name is required.");
 
        var documentExists = await documentRepository.ContainsDocumentId(documentId);
        if (!documentExists)
            throw new KeyNotFoundException($"Required document with ID {documentId} does not exist.");
 
        var programExists = await programRepository.ExistsAsync(request.ProgramId);
        if (!programExists)
            throw new KeyNotFoundException($"Program with ID {request.ProgramId} does not exist.");
 
        var programActive = await programRepository.IsActiveAsync(request.ProgramId);
        if (!programActive)
            throw new InvalidOperationException($"Program with ID {request.ProgramId} is inactive. Cannot update documents.");
 
        return await documentRepository.UpdateDocument(documentId, request);
    }
 
    public async Task<DeleteRequiredDocumentResponseDto> DeleteDocument(int documentId)
    {
        if (documentId <= 0)
            throw new ArgumentException("Document ID must be greater than zero.");
 
        var documentExists = await documentRepository.ContainsDocumentId(documentId);
        if (!documentExists)
            throw new KeyNotFoundException($"Required document with ID {documentId} does not exist.");
 
        return await documentRepository.DeleteDocument(documentId);
    }
}

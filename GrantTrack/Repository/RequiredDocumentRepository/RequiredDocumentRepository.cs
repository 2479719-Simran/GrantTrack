using AutoMapper;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.RequiredDocumentsDtos;
using Microsoft.EntityFrameworkCore;
 
namespace GrantTrack.Repository.RequiredDocumentRepositories;
 
public class RequiredDocumentRepository : IRequiredDocumentRepository
{
    private readonly GrantTrackDbContext dbContext;
    private readonly IMapper mapper;
 
    public RequiredDocumentRepository(GrantTrackDbContext dbContext, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }
 
    public async Task<bool> ContainsDocumentId(int documentId)
    {
        var obj = await dbContext.RequiredDocuments.FindAsync(documentId);
        return obj != null;
    }
 
    public async Task<bool> ContainsProgramId(int programId)
    {
        var obj = await dbContext.RequiredDocuments
                                 .FirstOrDefaultAsync(q => q.ProgramId == programId);
        return obj != null;
    }
 
    public async Task<CreateRequiredDocumentResponseDto> CreateDocument(CreateRequiredDocumentRequestDto request)
    {
        var newDocument = new RequiredDocument
        {
            ProgramId  = request.ProgramId,
            Name       = request.Name,
            Mandatory  = request.Mandatory
        };
 
        dbContext.Add(newDocument);
        await dbContext.SaveChangesAsync();
 
        return mapper.Map<CreateRequiredDocumentResponseDto>(newDocument);
    }
 
    public async Task<IEnumerable<GetRequiredDocumentResponseDto>> GetDocuments()
    {
        var documents = await dbContext.RequiredDocuments.ToListAsync();
        return mapper.Map<IEnumerable<GetRequiredDocumentResponseDto>>(documents);
    }
 
    public async Task<IEnumerable<GetRequiredDocumentResponseDto>> GetDocumentsByProgramId(int programId)
    {
        var documents = await dbContext.RequiredDocuments
                                       .Where(q => q.ProgramId == programId)
                                       .ToListAsync();
 
        return mapper.Map<IEnumerable<GetRequiredDocumentResponseDto>>(documents);
    }
 
    public async Task<UpdateRequiredDocumentResponseDto> UpdateDocument(int documentId, UpdateRequiredDocumentRequestDto request)
    {
        var obj = await dbContext.RequiredDocuments.FindAsync(documentId);
 
        obj!.ProgramId = request.ProgramId;
        obj.Name       = request.Name;
        obj.Mandatory  = request.Mandatory;
 
        await dbContext.SaveChangesAsync();
 
        return mapper.Map<UpdateRequiredDocumentResponseDto>(obj);
    }
 
    public async Task<DeleteRequiredDocumentResponseDto> DeleteDocument(int documentId)
    {
        var obj = await dbContext.RequiredDocuments.FindAsync(documentId);
 
        dbContext.RequiredDocuments.Remove(obj!);
        await dbContext.SaveChangesAsync();
 
        return mapper.Map<DeleteRequiredDocumentResponseDto>(obj);
    }
}
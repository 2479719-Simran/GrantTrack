using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.DocumentRepositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly GrantTrackDbContext _context;

    public DocumentRepository(GrantTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Document> CreateAsync(Document document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<Document?> GetByIdAsync(int documentId)
    {
        return await _context.Documents
            .Include(d => d.Application)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
    }

    public async Task<Document> UpdateAsync(Document document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<bool> ExistsForApplicationAsync(int applicationId, string fileName)
    {
        return await _context.Documents
            .AnyAsync(d => d.ApplicationId == applicationId && d.FileName == fileName);
    }
}
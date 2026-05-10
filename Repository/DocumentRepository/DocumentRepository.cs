using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.DocumentRepository;

public class DocumentRepository : IDocumentRepository
{
    private readonly GrantTrackDbContext _context;

    public DocumentRepository(GrantTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Document> AddAsync(Document doc, int applicationId)
    {
        var entry = _context.Documents.Add(doc);
        // EF discovered the FK column "ApplicationId" as a shadow property
        // because Document has a `public Application Application` nav prop
        // but no matching CLR FK property. The schema's NOT NULL constraint
        // means we must populate it via EntityEntry.Property() before save.
        entry.Property("ApplicationId").CurrentValue = applicationId;
        await _context.SaveChangesAsync();
        return doc;
    }

    public async Task<List<Document>> GetByApplicationIdAsync(int applicationId)
    {
        // The Documents table has BOTH a real FK ApplicationId AND a quirky
        // 'ApplicantionId' (typo) column. The FK column is what EF tracks,
        // so we filter by that — see Migrations/20260505122218_first.cs.
        return await _context.Documents
            .Where(d => EF.Property<int>(d, "ApplicationId") == applicationId)
            .ToListAsync();
    }

    public Task<Document?> GetByIdAsync(int documentId) =>
        _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId);

    public async Task DeleteAsync(Document doc)
    {
        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync();
    }

    public Task<int?> GetApplicantIdForApplicationAsync(int applicationId) =>
        _context.Applications
            .Where(a => a.ApplicationId == applicationId)
            .Select(a => (int?)a.ApplicantId)
            .FirstOrDefaultAsync();

    public Task<bool> IsReviewerAssignedAsync(int applicationId, int userId) =>
        _context.Reviews
            .AnyAsync(r => r.ApplicationId == applicationId && r.ReviewerId == userId);
}

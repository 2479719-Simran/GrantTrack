using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.DocumentRepositories;

public interface IDocumentRepository
{
    Task<Document> CreateAsync(Document document);
    Task<Document?> GetByIdAsync(int documentId);
    Task<Document> UpdateAsync(Document document);
    Task<bool> ExistsForApplicationAsync(int applicationId, string fileName);
}
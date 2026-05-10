using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.DocumentRepository;

public interface IDocumentRepository
{
    /// <summary>
    /// Persists a new document, populating BOTH the typo'd ApplicantionId
    /// column AND the shadow FK ApplicationId column. The schema has both
    /// (see Migrations/20260505122218_first.cs); both are NOT NULL.
    /// </summary>
    Task<Document> AddAsync(Document doc, int applicationId);
    Task<List<Document>> GetByApplicationIdAsync(int applicationId);
    Task<Document?> GetByIdAsync(int documentId);
    Task DeleteAsync(Document doc);
    /// <summary>
    /// Light projection so we can verify ownership of an application without
    /// loading the full graph from EF.
    /// </summary>
    Task<int?> GetApplicantIdForApplicationAsync(int applicationId);

    /// <summary>
    /// True when the given user has at least one Review row for the given
    /// application — i.e. the Admin assigned them to it. Used by the
    /// document service to gate read access for the Reviewer role.
    /// </summary>
    Task<bool> IsReviewerAssignedAsync(int applicationId, int userId);
}

using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.DecisionRepositories
{
    /// <summary>
    /// Defines operations for managing application decisions.
    /// </summary>
    public interface IDecisionRepository
    {
        Task<bool> DecisionExistsAsync(int applicationId);
        Task AddDecisionAsync(Decision decision);
    }
}

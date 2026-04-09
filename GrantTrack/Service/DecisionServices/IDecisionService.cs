using GrantTrack.Dto.DecisionDtos;

namespace GrantTrack.Service.DecisionServices
{
    /// <summary>
    /// Defines business logic for processing application decisions.
    /// </summary>
    public interface IDecisionService
    {
        Task CreateDecisionAsync(DecisionDto dto);
    }
}
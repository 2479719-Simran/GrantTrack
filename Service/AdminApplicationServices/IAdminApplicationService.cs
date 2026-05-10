using GrantTrack.Dto.AdminDtos;

namespace GrantTrack.Service.AdminApplicationServices;

public interface IAdminApplicationService
{
    /// <summary>
    /// All applications across the system, oldest-submitted first, hydrated
    /// with applicant + program info plus pipeline rollups (reviewer count,
    /// recommendation count, doc count, final decision).
    /// </summary>
    Task<List<AdminApplicationListDto>> ListAllAsync();

    /// <summary>
    /// Full detail for a single application — applicant, program, reviewers,
    /// recommendations, final decision (if any), documents.
    /// Returns null if the id doesn't exist.
    /// </summary>
    Task<AdminApplicationDetailDto?> GetDetailAsync(int applicationId);
}

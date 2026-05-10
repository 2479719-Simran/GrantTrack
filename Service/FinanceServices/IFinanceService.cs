using GrantTrack.Dto.FinanceDtos;

namespace GrantTrack.Service.FinanceServices;

public interface IFinanceService
{
    /// <summary>
    /// All approved applications (Decision row exists with value Approved),
    /// hydrated with applicant + program info plus disbursement rollups
    /// (count, total disbursed, total paid, remaining headroom, status mix).
    /// </summary>
    Task<List<FinanceApplicationListDto>> ListApprovedAsync();

    /// <summary>
    /// Full detail for a single application: header + decision context +
    /// every disbursement with its payments. Returns null if the id doesn't
    /// resolve to an approved application.
    /// </summary>
    Task<FinanceApplicationDetailDto?> GetDetailAsync(int applicationId);

    /// <summary>
    /// Just the disbursement + payment data for an application — the read
    /// shape consumed by the applicant + admin detail pages so they can
    /// render the funding state without going through the full Finance
    /// Officer DTO. Returns an empty list (not null) when the application
    /// has no tranches yet.
    /// </summary>
    Task<List<FinanceDisbursementDto>> ListDisbursementsForApplicationAsync(int applicationId);

    /// <summary>
    /// Verifies the given user owns the given application — used by the
    /// shared disbursement endpoint to gate Applicant access (every other
    /// privileged role gets blanket read).
    /// </summary>
    Task<int?> GetApplicantIdAsync(int applicationId);
}

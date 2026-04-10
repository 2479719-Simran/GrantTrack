using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.DisbursementRepositories;

public interface IDisbursementRepository
{
    Task<Disbursement> CreateAsync(Disbursement disbursement);
    Task<Disbursement?> GetByIdAsync(int id);
    Task<Disbursement> UpdateAsync(Disbursement disbursement);
    Task<decimal> GetTotalDisbursedAmountAsync(int applicationId);
    Task<Application?> GetApplicationWithProgramAsync(int applicationId);
}

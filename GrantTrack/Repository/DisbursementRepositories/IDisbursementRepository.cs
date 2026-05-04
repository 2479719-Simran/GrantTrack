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
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task<decimal> GetTotalPaidAmountAsync(int disbursementId);
    Task<(IEnumerable<Disbursement> Items, int TotalCount)> GetFilteredDisbursementsAsync(
        int? applicationId, string? status, int page, int pageSize);
    Task<(IEnumerable<Payment> Items, int TotalCount)> GetFilteredPaymentsAsync(
        DateTime? from, DateTime? to, int page, int pageSize);
}

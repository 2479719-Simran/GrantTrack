using System;
using GrantTrack.Domain.Entities;
using AppEntity = GrantTrack.Domain.Entities.Application;
namespace GrantTrack.Repository.DisbursementRepositories;

public interface IDisbursementRepository
{
    Task<Disbursement> CreateAsync(Disbursement disbursement);
    Task<Disbursement?> GetByIdAsync(int id);
    Task<Disbursement> UpdateAsync(Disbursement disbursement);
    Task<decimal> GetTotalDisbursedAmountAsync(int applicationId);
    Task<AppEntity?> GetApplicationWithProgramAsync(int applicationId);
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task<decimal> GetTotalPaidAmountAsync(int disbursementId);
}

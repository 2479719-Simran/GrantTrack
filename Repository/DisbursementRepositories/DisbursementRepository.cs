using System;
using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AppEntity = GrantTrack.Domain.Entities.Application;
namespace GrantTrack.Repository.DisbursementRepositories;

public class DisbursementRepository : IDisbursementRepository
{
    private readonly GrantTrackDbContext _context;

    public DisbursementRepository(GrantTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Disbursement> CreateAsync(Disbursement disbursement)
    {
        _context.Disbursements.Add(disbursement);
        await _context.SaveChangesAsync();
        return disbursement;
    }

    public async Task<Disbursement?> GetByIdAsync(int id)
    {
        return await _context.Disbursements
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DisbursementId == id);
    }

    public async Task<Disbursement> UpdateAsync(Disbursement disbursement)
    {
        _context.Disbursements.Update(disbursement);
        await _context.SaveChangesAsync();
        return disbursement;
    }
    public async Task<decimal> GetTotalDisbursedAmountAsync(int applicationId)
    {
        return await _context.Disbursements
            .Where(d => d.ApplicationId == applicationId
                     && d.Status != DisbursementStatus.Cancelled)
            .SumAsync(d => (decimal?)d.Amount) ?? 0;
    }
    public async Task<AppEntity?> GetApplicationWithProgramAsync(int applicationId)
    {
    return await _context.Applications
        .Include(a => a.ProgramIDNavigation)
        .FirstOrDefaultAsync(a => a.ApplicationId == applicationId); 
    }

      public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        _context.payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<decimal> GetTotalPaidAmountAsync(int disbursementId)
    {
        return await _context.payments
            .Where(p => p.DisbursementId == disbursementId
                     && p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;
    }
}
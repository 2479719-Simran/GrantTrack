using System;
using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
    public async Task<Application?> GetApplicationWithProgramAsync(int applicationId)
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
    public async Task<(IEnumerable<Disbursement> Items, int TotalCount)> GetFilteredDisbursementsAsync(
        int? applicationId, string? status, int page, int pageSize)
    {
        var query = _context.Disbursements.AsNoTracking().AsQueryable();

        if (applicationId.HasValue)
            query = query.Where(d => d.ApplicationId == applicationId.Value);

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<DisbursementStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(d => d.Status == parsedStatus);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.ScheduledDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    public async Task<(IEnumerable<Payment> Items, int TotalCount)> GetFilteredPaymentsAsync(
        DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = _context.payments.AsNoTracking().AsQueryable();

        if (from.HasValue)
            query = query.Where(p => p.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.Date <= to.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
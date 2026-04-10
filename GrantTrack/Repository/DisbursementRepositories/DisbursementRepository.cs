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
}
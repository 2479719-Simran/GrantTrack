using GrantTrack.Domain.Entities;
using GrantTrack.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using AppEntity = GrantTrack.Domain.Entities.Application; 

namespace GrantTrack.Repository.ApplicationRepositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly GrantTrackDbContext _db;
    public ApplicationRepository(GrantTrackDbContext db) => _db = db;

    public async Task<AppEntity> CreateAsync(AppEntity application)
    {
        _db.Applications.Add(application);
        await _db.SaveChangesAsync();
        return application;
    }

    public async Task<AppEntity?> GetByIdAsync(int id)
        => await _db.Applications
            .Include(a => a.ProgramIDNavigation)
            .Include(a => a.ApplicantIDNavigation)
            .FirstOrDefaultAsync(a => a.ApplicationId == id);

    public async Task<AppEntity> UpdateAsync(AppEntity application)
    {
        _db.Applications.Update(application);
        await _db.SaveChangesAsync();
        return application;
    }

    public async Task<bool> ExistsForApplicantAsync(int applicantId, int programId)
        => await _db.Applications.AnyAsync(a => a.ApplicantId == applicantId && a.ProgramId == programId);

    public async Task<AppEntity> GetApplicationByIdAsync(int id)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.ApplicationId == id);
        if (application == null) throw new Exception("Not found");
        return application;
    }
}
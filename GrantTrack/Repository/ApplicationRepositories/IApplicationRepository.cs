using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.ApplicationRepositories;

public interface IApplicationRepository
{
    Task<Application> CreateAsync(Application application);
    Task<Application?> GetByIdAsync(int id);
    Task<Application> UpdateAsync(Application application);
  
}

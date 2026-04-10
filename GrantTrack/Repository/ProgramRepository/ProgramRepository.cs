using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ProgramDtos;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.ProgramRepository;

public class ProgramRepository : IProgramRepository
{
    private readonly GrantTrackDbContext dbContext;

    public ProgramRepository(GrantTrackDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<bool> ContainsName(string Name)
    {
        var obj = await dbContext.GrantPrograms.FirstOrDefaultAsync(q => q.Name == Name);

        if (obj == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public async Task<bool> ContainsId(int id)
    {
        var obj = await dbContext.GrantPrograms.FindAsync(id);

        if (obj == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async  Task<GrantProgram> ProgramWithId(int id)
    {
        var obj = await dbContext.GrantPrograms.FindAsync(id);  

        return obj; 
    }
    public async Task<CreateProgramResponseDto> CreateProgram(CreateProgramRequestDto request)
    {
        var newProgram = new GrantProgram
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Budget = request.Budget,
            Status = request.Status
        };
        dbContext.Add(newProgram);
        await dbContext.SaveChangesAsync();

        return new CreateProgramResponseDto
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Budget = request.Budget,
            Status = request.Status
        };
    }

    public async Task<DeleteProgramDto> DeleteProgram(int id)
    {
        var obj = await dbContext.GrantPrograms.FindAsync(id);
        obj.Status = false;
        await dbContext.SaveChangesAsync();

        return new DeleteProgramDto
        {
            ProgramId = obj.ProgramId,
            Name = obj.Name,
            Description = obj.Description,
            Budget = obj.Budget,
            EndDate = obj.EndDate,
            StartDate = obj.StartDate,
            Status = obj.Status
        };
    }

    public async Task<IEnumerable<GetProgramDto>> GetPrograms()
    {
        List<GetProgramDto> response = new List<GetProgramDto>();
        var grantPrograms = await dbContext.GrantPrograms.ToListAsync(); 
        foreach (var program in grantPrograms)
        {
            response.Add(new GetProgramDto
            {
                ProgramId = program.ProgramId,
                Name = program.Name,
                Budget = program.Budget,
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                Description = program.Description,
                Status = program.Status

            });
        }
        return response;
    }

    public async Task<UpdateProgramResponseDto> UpdateProgram(int id, UpdateProgramRequestDto request)
    {
        var obj = await dbContext.GrantPrograms.FindAsync(id);

        obj.Name = request.Name;
        obj.Description = request.Description;
        obj.Budget = request.Budget;
        obj.StartDate = request.StartDate;
        obj.EndDate = request.EndDate;
        obj.Status = request.Status;

        await dbContext.SaveChangesAsync();
        return new UpdateProgramResponseDto
        {
            ProgramId = obj.ProgramId,
            Name = obj.Name,
            Description = obj.Description,
            Budget = obj.Budget,
            EndDate = obj.EndDate,
            StartDate = obj.StartDate,
            Status = obj.Status
        };


    }

    public async Task<IEnumerable<GetProgramDto>> FilterPrograms(FilterProgramsDto request)
    {
        var query = dbContext.GrantPrograms.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(q => q.Status == bool.Parse(request.Status.ToLower()));
        }
        if (request.StartDate.HasValue)
        {
            query = query.Where(q => request.StartDate <= q.StartDate);
        }
        if (request.EndDate.HasValue)
        {
            query = query.Where(q => request.EndDate >= q.EndDate);
        }
        var grantPrograms = await query.ToListAsync();
        List<GetProgramDto> response = new List<GetProgramDto>(); 
        foreach (var program in grantPrograms)
        {
            response.Add(new GetProgramDto
            {
                ProgramId = program.ProgramId,
                Name = program.Name,
                Budget = program.Budget,
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                Description = program.Description,
                Status = program.Status

            });
        }
        return response;
    }
     // Returns true if a program row with this ID exists.
    public async Task<bool> ExistsAsync(int programId)
        => await dbContext.GrantPrograms.AnyAsync(p => p.ProgramId == programId);

    // Returns true only when the program exists AND Status == true (active).
    public async Task<bool> IsActiveAsync(int programId)
        => await dbContext.GrantPrograms.AnyAsync(p => p.ProgramId == programId && p.Status);

    
}

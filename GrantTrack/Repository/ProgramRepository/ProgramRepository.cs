using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ProgramDtos;
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

        if(obj == null)
        {
            return false;
        }
        else
        {
            return true; 
        }
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

    public Task<DeleteProgramDto> DeleteProgram()
    {
        throw new NotImplementedException();
    }

    public Task<GetProgramDto> GetProgramById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<GetProgramDto> GetPrograms()
    {
        throw new NotImplementedException();
    }

    public Task<UpdateProgramResponseDto> UpdateProgram(int id, UpdateProgramRequestDto request)
    {
        throw new NotImplementedException();
    }
}

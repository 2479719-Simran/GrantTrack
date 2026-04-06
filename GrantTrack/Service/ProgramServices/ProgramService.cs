using System;
using GrantTrack.Dto.ProgramDtos;
using GrantTrack.Repository.ProgramRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Client;

namespace GrantTrack.Service.ProgramServices;

public class ProgramService : IProgramService
{
    private readonly IProgramRepository programRepository;

    public ProgramService(IProgramRepository programRepository)
    {
        this.programRepository = programRepository;
    }
    public async Task<CreateProgramResponseDto> CreateProgram(CreateProgramRequestDto request)
    {
        if (request.Name == null || request.Name.Length == 0)
        {
            throw new ArgumentNullException("Name cannot be Empty");
        }
        if (request.Description == null || request.Description.Length == 0)
        {
            throw new ArgumentNullException("Description cannot be Empty");
        }
        if (request.Budget == 0)
        {
            throw new ArgumentNullException("Budget cannot be zero");
        }
        if (request.Status != true && request.Status != false)
        {
            throw new ArgumentException("Budget cannot be zero");

        }
        if (request.StartDate > request.EndDate)
        {
            throw new ArgumentException("Start Date Cannot be present after End date");
        }

        var containsName = await programRepository.ContainsName(request.Name);

        if (containsName)
        {
            throw new InvalidOperationException($"Program {request.Name}Name is already Present");
        } 
        return await programRepository.CreateProgram(request);

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

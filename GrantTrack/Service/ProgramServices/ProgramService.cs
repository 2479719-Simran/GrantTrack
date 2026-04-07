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
            throw new ArgumentException("Start Date Cannot be after End date");
        }

        var containsName = await programRepository.ContainsName(request.Name);

        if (containsName)
        {
            throw new InvalidOperationException($"Program {request.Name} Name is already Present");
        }
        return await programRepository.CreateProgram(request);

    }

    public async Task<DeleteProgramDto> DeleteProgram(int id)
    {
        bool idIsPresent = await programRepository.ContainsId(id);

        if (!idIsPresent)
        {
            throw new ArgumentException($"Program with Id {id} is not present");
        }

        return await programRepository.DeleteProgram(id);

    }

    public Task<GetProgramDto> GetProgramById(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<GetProgramDto>> GetPrograms(bool? Status, DateTime? StartDate, DateTime? EndDate)
    {
        if (Status.HasValue && Status != true && Status != false)
        {
            throw new ArgumentException("Status has to be true or false");

        }
        if (StartDate > EndDate)
        {
            throw new ArgumentException("Start Date Cannot be after End date");
        }
        return await programRepository.GetPrograms(Status, StartDate, EndDate);
    }

    public async Task<UpdateProgramResponseDto> UpdateProgram(int id, UpdateProgramRequestDto request)
    {
        bool idIsPresent = await programRepository.ContainsId(id);

        if (!idIsPresent)
        {
            throw new ArgumentException($"Program with Id {id} is not present");
        }
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
            throw new ArgumentException("Start Date Cannot be after End date");
        }

        var containsName = await programRepository.ContainsName(request.Name);

        if (containsName)
        {
            throw new InvalidOperationException($"Program {request.Name} Name is already Present");
        }

        return await programRepository.UpdateProgram(id, request);
    }
}

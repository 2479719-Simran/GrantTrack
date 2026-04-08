using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ProgramDtos;

namespace GrantTrack.Repository.ProgramRepository;

public interface IProgramRepository
{
    Task<CreateProgramResponseDto> CreateProgram(CreateProgramRequestDto request); 
    Task<bool> ContainsName(string Name); 
    Task<bool> ContainsId(int id); 
    Task<IEnumerable<GetProgramDto>> GetPrograms(); 
    Task<IEnumerable<GetProgramDto>> FilterPrograms(FilterProgramsDto request);
    Task<UpdateProgramResponseDto> UpdateProgram(int id , UpdateProgramRequestDto request);
    Task<GrantProgram> ProgramWithId(int id);
    Task<DeleteProgramDto> DeleteProgram(int id);
}

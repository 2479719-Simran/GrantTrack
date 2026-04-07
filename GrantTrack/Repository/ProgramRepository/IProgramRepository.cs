using System;
using GrantTrack.Dto.ProgramDtos;

namespace GrantTrack.Repository.ProgramRepository;

public interface IProgramRepository
{
    Task<CreateProgramResponseDto> CreateProgram(CreateProgramRequestDto request); 
    Task<bool> ContainsName(string Name); 
    Task<bool> ContainsId(int id); 
    Task<IEnumerable<GetProgramDto>> GetPrograms(bool? Status ,DateTime? StartDate ,DateTime? EndDate); 
    Task<GetProgramDto> GetProgramById(int id); 
    Task<UpdateProgramResponseDto> UpdateProgram(int id , UpdateProgramRequestDto request);
    Task<DeleteProgramDto> DeleteProgram(int id);
}

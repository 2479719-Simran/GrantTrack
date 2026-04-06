using System;
using GrantTrack.Dto.ProgramDtos;

namespace GrantTrack.Repository.ProgramRepository;

public interface IProgramRepository
{
    Task<CreateProgramResponseDto> CreateProgram(CreateProgramRequestDto request); 
    Task<bool> ContainsName(string Name); 
    Task<GetProgramDto> GetPrograms(); 
    Task<GetProgramDto> GetProgramById(int id); 
    Task<UpdateProgramResponseDto> UpdateProgram(int id , UpdateProgramRequestDto request);
    Task<DeleteProgramDto> DeleteProgram();
}

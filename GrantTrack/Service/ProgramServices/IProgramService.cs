using System;
using GrantTrack.Dto.ProgramDtos;
namespace GrantTrack.Service.ProgramServices;

public interface IProgramService
{
    Task<CreateProgramResponseDto> CreateProgram(CreateProgramRequestDto request); 
    Task<GetProgramDto> GetPrograms(); 
    Task<GetProgramDto> GetProgramById(int id); 
    Task<UpdateProgramResponseDto> UpdateProgram(int id , UpdateProgramRequestDto request);
    Task<DeleteProgramDto> DeleteProgram();
}

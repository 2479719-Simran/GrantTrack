using System;
using GrantTrack.Dto.ProgramDtos;
namespace GrantTrack.Service.ProgramServices;

public interface IProgramService
{
    Task<CreateProgramResponseDto> CreateProgram(CreateProgramRequestDto request); 
    Task<IEnumerable<GetProgramDto>> GetPrograms( ); 
    Task<IEnumerable<GetProgramDto>> FilterPrograms(FilterProgramsDto request);
    Task<UpdateProgramResponseDto> UpdateProgram(int id , UpdateProgramRequestDto request);
    Task<DeleteProgramDto> DeleteProgram(int id);
}

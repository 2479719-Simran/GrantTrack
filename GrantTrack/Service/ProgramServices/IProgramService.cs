using System;
using GrantTrack.Dto.ProgramDtos;
namespace GrantTrack.Service.ProgramServices;

public interface IProgramService
{
    Task<CreateProgramResponseDto> CreateProgram(CreateProgramRequestDto request); 
    Task<IEnumerable<GetProgramDto>> GetPrograms( bool? Status ,DateTime? StartDate ,DateTime? EndDate); 
    Task<GetProgramDto> GetProgramById(int id); 
    Task<UpdateProgramResponseDto> UpdateProgram(int id , UpdateProgramRequestDto request);
    Task<DeleteProgramDto> DeleteProgram(int id);
}

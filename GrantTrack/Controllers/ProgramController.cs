using GrantTrack.Dto.ProgramDtos;
using GrantTrack.Repository.ProgramRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProgramController : ControllerBase
    {
        private readonly IProgramRepository programRepository;

        public ProgramController(IProgramRepository programRepository)
        {
            this.programRepository = programRepository; 
        }
        [HttpPost("CreateProgram")]  
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] 

        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramRequestDto request)
        {
            try
            {   var created = await programRepository.CreateProgram(request); 
                return StatusCode(201,created); 
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            } 
            catch(InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch(Exception)
            {
                return StatusCode(500, "Internal Server error occured"); 
            }

        } 
        // [HttpGet("GetPrograms")] 
        // public async Task<IActionResult> GetPrograms()
        // {
            
        // } 
        // [HttpGet("GetPrograms/{id}")] 
        // public async Task<IActionResult> GetProgramById([FromRoute]int id )
        // {
            
        // } 
        // [HttpPut("UpdateProgram/{id}")] 
        // public async Task<IActionResult> UpdateProgram([FromRoute]int id , [FromBody] UpdateProgramRequestDto request)
        // {
            
        // }
        // [HttpDelete("{id}")] 
        // public async Task<IActionResult> DeleteProgram([FromRoute]int id)
        // {
            
        // }



    }
}

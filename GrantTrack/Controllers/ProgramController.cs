using GrantTrack.Dto.ProgramDtos;
using GrantTrack.Repository.ProgramRepository;
using GrantTrack.Service.ProgramServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProgramController : ControllerBase
    {
        private readonly IProgramService programService;

        public ProgramController(IProgramService programService)
        {
            this.programService = programService;
        }
        [HttpPost("CreateProgram")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramRequestDto request)
        {
            try
            {
                var created = await programService.CreateProgram(request);
                return StatusCode(201, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server error occured");
            }

        }
        [HttpGet("GetPrograms")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPrograms([FromQuery] bool? Status, [FromQuery] DateTime? StartDate, [FromQuery] DateTime? EndDate)
        {
            try
            {
                var grantPrograms = await programService.GetPrograms(Status, StartDate, EndDate);
                return Ok(grantPrograms);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server error occured");
            }
        }
        // [HttpGet("GetPrograms/{id}")] 
        // public async Task<IActionResult> GetProgramById([FromRoute]int id )
        // {

        // } 
        [HttpPut("UpdateProgram/{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProgram([FromRoute] int id, [FromBody] UpdateProgramRequestDto request)
        {
            try
            {
                var updated = await programService.UpdateProgram(id, request);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server error occured");
            }


        }
        [HttpDelete("DeleteProgram/{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProgram([FromRoute] int id)
        {
            try
            {
                var deleted = await programService.DeleteProgram(id);
                return Ok(deleted);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server error occured");
            }
        }



    }
}

using GrantTrack.Domain.Entities;
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
        [Authorize(Roles = nameof(UserRole.Admin))]
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
        // Any authenticated user can browse programs. Applicants need this
        // to know what to apply to; reviewers / approvers / finance / compliance
        // need it for context on the applications they handle. Admin still
        // owns the create/update/delete endpoints below.
        [Authorize]
        public async Task<IActionResult> GetPrograms()
        {
            try
            {
                var grantPrograms = await programService.GetPrograms();
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
         
        [HttpPost("FilterPrograms")] 
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> FilterPrograms([FromBody] FilterProgramsDto request)
        {
            try
            {
                var programs = await programService.FilterPrograms(request);
                return StatusCode(201, programs);
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
        [HttpPut("UpdateProgram/{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
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
        [Authorize(Roles = nameof(UserRole.Admin))]
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

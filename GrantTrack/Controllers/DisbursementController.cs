using System.Net;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DisbursementDtos;
using GrantTrack.Service.DisbursementServices;
using GrantTrack.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = nameof(UserRole.FinanceOfficer))]
    public class DisbursementController : ControllerBase
    {
        private readonly IDisbursementService _disbursementService;

        /// <summary>
        /// The DisbursementController handles disbursement tranche operations.
        /// It provides endpoints for Finance Officers to create and update disbursement tranches.
        /// </summary>
        /// <param name="disbursementService">The disbursement service instance</param>
        public DisbursementController(IDisbursementService disbursementService)
        {
            _disbursementService = disbursementService;
        }

        /// <summary>
        /// POST /api/v1/disbursement
        /// Finance Officer creates a new disbursement tranche for an approved application.
        /// </summary>
        /// <param name="dto">The create disbursement request DTO</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateDisbursement([FromBody] CreateDisbursementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _disbursementService.CreateDisbursementAsync(dto);
                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = Messages.DisbursementCreated,
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)  // ← THIS WAS MISSING
            {
              return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = Messages.SomethingWentWrong });
            }
        }

        /// <summary>
        /// PATCH /api/v1/disbursement/{id}
        /// Finance Officer updates an existing tranche.
        /// Emits Disbursement.Scheduled when status transitions to Scheduled.
        /// </summary>
        /// <param name="id">The disbursement ID</param>
        /// <param name="dto">The update disbursement request DTO</param>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDisbursement(
            [FromRoute] int id,
            [FromBody] UpdateDisbursementDto dto)
        {
            if (dto == null)
                return BadRequest(new { error = Messages.InvalidRequest });

            try
            {
                var result = await _disbursementService.UpdateDisbursementAsync(id, dto);

                if (result == null)
                    return NotFound(new { message = Messages.DisbursementNotFound });

                return Ok(new
                {
                    message = Messages.DisbursementUpdated,
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = Messages.SomethingWentWrong });
            }
        }
        
        /// <summary>
        /// POST /api/v1/disbursement/payments
        /// Finance Officer records a payment against a disbursement.
        /// Emits Payment.Recorded event.
        /// </summary>
        [HttpPost("payments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _disbursementService.CreatePaymentAsync(dto);
                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = Messages.PaymentCreated,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == Messages.PaymentDisbursementAlreadyPaid)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = Messages.SomethingWentWrong });
            }
        }
    }
}

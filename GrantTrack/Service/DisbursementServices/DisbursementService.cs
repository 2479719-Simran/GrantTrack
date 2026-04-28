using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DisbursementDtos;
using GrantTrack.Repository.DisbursementRepositories;
using GrantTrack.Utility;
using Microsoft.Extensions.Logging;

namespace GrantTrack.Service.DisbursementServices;

public class DisbursementService : IDisbursementService  
{
    private readonly IDisbursementRepository _disbursementRepository;
    private readonly ILogger<DisbursementService> _logger;

    public DisbursementService(
        IDisbursementRepository disbursementRepository,
        ILogger<DisbursementService> logger)
    {
        _disbursementRepository = disbursementRepository;
        _logger = logger;
    }

    public async Task<DisbursementResponseDto> CreateDisbursementAsync(CreateDisbursementDto dto)
    {
        if (dto.ScheduledDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException(Messages.DisbursementScheduledDateInPast);

        var application = await _disbursementRepository.GetApplicationWithProgramAsync(dto.ApplicationId);
        if (application == null)
            throw new ArgumentException(Messages.ApplicationNotFound);

        var programBudget  = application.ProgramIDNavigation.Budget;
        var totalDisbursed = await _disbursementRepository.GetTotalDisbursedAmountAsync(dto.ApplicationId);
        var remaining      = programBudget - totalDisbursed;

        if (dto.Amount > remaining)
            throw new InvalidOperationException(
                string.Format(Messages.DisbursementExceedsBudget, remaining));

        var entity = new Disbursement
        {
            ApplicationId = dto.ApplicationId,
            Amount        = dto.Amount,
            ScheduledDate = dto.ScheduledDate,
            Status        = DisbursementStatus.Pending
        };

        var created = await _disbursementRepository.CreateAsync(entity);
        return MapToResponse(created);
    }

    public async Task<DisbursementResponseDto?> UpdateDisbursementAsync(int id, UpdateDisbursementDto dto)
    {
        var existing = await _disbursementRepository.GetByIdAsync(id);
        if (existing == null) return null;

        if (existing.Status == DisbursementStatus.Paid ||
            existing.Status == DisbursementStatus.Cancelled)
            throw new InvalidOperationException(Messages.DisbursementCannotBeModified);

        // ── Budget validation when amount is updated ─────────────────────
        if (dto.Amount.HasValue)
        {
            var application   = await _disbursementRepository.GetApplicationWithProgramAsync(existing.ApplicationId);
            var programBudget = application!.ProgramIDNavigation.Budget;

            // Exclude current disbursement from total before comparing
            var totalExcludingCurrent = await _disbursementRepository.GetTotalDisbursedAmountAsync(existing.ApplicationId)
                                        - existing.Amount;
            var remaining = programBudget - totalExcludingCurrent;

            if (dto.Amount.Value > remaining)
                throw new InvalidOperationException(
                    string.Format(Messages.DisbursementExceedsBudget, remaining));

            existing.Amount = dto.Amount.Value;
        }

        if (dto.ScheduledDate.HasValue) existing.ScheduledDate = dto.ScheduledDate.Value;
        if (dto.ActualDate.HasValue)    existing.ActualDate    = dto.ActualDate.Value;

        bool becameScheduled = false;

        if (dto.Status.HasValue)
        {
            ValidateStatusTransition(existing.Status, dto.Status.Value);
            becameScheduled = dto.Status.Value == DisbursementStatus.Scheduled
                              && existing.Status != DisbursementStatus.Scheduled;
            existing.Status = dto.Status.Value;
        }

        var updated = await _disbursementRepository.UpdateAsync(existing);

        if (becameScheduled)
            EmitDisbursementScheduled(updated);

        return MapToResponse(updated);
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var disbursement = await _disbursementRepository.GetByIdAsync(dto.DisbursementId);
        if (disbursement == null)
            throw new KeyNotFoundException(Messages.DisbursementNotFound);

        if (disbursement.Status == DisbursementStatus.Paid)
            throw new InvalidOperationException(Messages.PaymentDisbursementAlreadyPaid);

        if (disbursement.Status != DisbursementStatus.Scheduled &&
            disbursement.Status != DisbursementStatus.PartiallyPaid)
            throw new InvalidOperationException(Messages.PaymentDisbursementNotScheduled);

        var totalPaid = await _disbursementRepository.GetTotalPaidAmountAsync(dto.DisbursementId);
        var remaining = disbursement.Amount - totalPaid;

        if (dto.Amount > remaining)
            throw new InvalidOperationException(
                string.Format(Messages.PaymentExceedsDisbursementAmount, remaining));
        // Create payment
        var entity = new Payment
        {
            DisbursementId = dto.DisbursementId,
            Amount         = dto.Amount,
            Date           = dto.Date,
            Method         = dto.Method,
            Status         = PaymentStatus.Completed
        };

        var created = await _disbursementRepository.CreatePaymentAsync(entity);

        var newTotalPaid = totalPaid + dto.Amount;

        var disbursementForUpdate = await _disbursementRepository.GetByIdAsync(dto.DisbursementId);
        if (disbursementForUpdate != null)
        {
            disbursementForUpdate.Status = newTotalPaid >= disbursementForUpdate.Amount
                ? DisbursementStatus.Paid
                : DisbursementStatus.PartiallyPaid;

            await _disbursementRepository.UpdateAsync(disbursementForUpdate);
        }

        // ── Emit Payment.Recorded event ──────────────────────────────────
        EmitPaymentRecorded(created);

        return MapPaymentToResponse(created);
    }
    
public async Task<PagedResponseDto<DisbursementResponseDto>> GetDisbursementsAsync(
    int? applicationId, string? status, int page, int pageSize)
{
    var (items, totalCount) = await _disbursementRepository
        .GetFilteredDisbursementsAsync(applicationId, status, page, pageSize);

    return new PagedResponseDto<DisbursementResponseDto>
    {
        Page         = page,
        PageSize     = pageSize,
        TotalRecords = totalCount,
        TotalPages   = (int)Math.Ceiling((double)totalCount / pageSize),
        Data         = items.Select(MapToResponse)
    };
}

public async Task<PagedResponseDto<PaymentResponseDto>> GetPaymentsAsync(
    DateTime? from, DateTime? to, int page, int pageSize)
{
    var (items, totalCount) = await _disbursementRepository
        .GetFilteredPaymentsAsync(from, to, page, pageSize);

    return new PagedResponseDto<PaymentResponseDto>
    {
        Page         = page,
        PageSize     = pageSize,
        TotalRecords = totalCount,
        TotalPages   = (int)Math.Ceiling((double)totalCount / pageSize),
        Data         = items.Select(MapPaymentToResponse)
    };
}
    // ── Helpers ─────────────────────────────────────────────────────────

    private void EmitDisbursementScheduled(Disbursement d)
    {
        _logger.LogInformation(
            "[Event: Disbursement.Scheduled] DisbursementId={DisbursementId} " +
            "ApplicationId={ApplicationId} Amount={Amount} " +
            "ScheduledDate={ScheduledDate} OccurredAt={OccurredAt}",
            d.DisbursementId, d.ApplicationId, d.Amount,
            d.ScheduledDate, DateTime.UtcNow);
    }

    private void EmitPaymentRecorded(Payment p)
    {
        _logger.LogInformation(
            "[Event: Payment.Recorded] PaymentId={PaymentId} " +
            "DisbursementId={DisbursementId} Amount={Amount} " +
            "Method={Method} OccurredAt={OccurredAt}",
            p.PaymentId, p.DisbursementId, p.Amount,
            p.Method, DateTime.UtcNow);
    }
    private static void ValidateStatusTransition(DisbursementStatus current, DisbursementStatus next)
    {
        var allowed = new Dictionary<DisbursementStatus, HashSet<DisbursementStatus>>
        {
            [DisbursementStatus.Pending]   = new() { DisbursementStatus.Scheduled, DisbursementStatus.Cancelled },
            [DisbursementStatus.Scheduled] = new() { DisbursementStatus.Paid, DisbursementStatus.PartiallyPaid, DisbursementStatus.Cancelled },
            [DisbursementStatus.PartiallyPaid] = new() { DisbursementStatus.Paid, DisbursementStatus.Cancelled }
        };

        if (!allowed.TryGetValue(current, out var validNext) || !validNext.Contains(next))
            throw new InvalidOperationException(
                string.Format(Messages.DisbursementInvalidStatusTransition, current, next));
    }

    private static DisbursementResponseDto MapToResponse(Disbursement d) => new()
    {
        DisbursementId = d.DisbursementId,
        ApplicationId  = d.ApplicationId,
        Amount         = d.Amount,
        ScheduledDate  = d.ScheduledDate,
        ActualDate     = d.ActualDate,
        Status         = d.Status.ToString()
    };
    private static PaymentResponseDto MapPaymentToResponse(Payment p) => new()
    {
        PaymentId      = p.PaymentId,
        DisbursementId = p.DisbursementId,
        Amount         = p.Amount,
        Date           = p.Date,
        Method         = p.Method.ToString(),
        Status         = p.Status.ToString()
    };
}
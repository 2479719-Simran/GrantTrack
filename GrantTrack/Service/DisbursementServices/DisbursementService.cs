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

        if (dto.Amount.HasValue)        existing.Amount        = dto.Amount.Value;
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

    private void EmitDisbursementScheduled(Disbursement d)
    {
        _logger.LogInformation(
            "[Event: Disbursement.Scheduled] DisbursementId={DisbursementId} " +
            "ApplicationId={ApplicationId} Amount={Amount} " +
            "ScheduledDate={ScheduledDate} OccurredAt={OccurredAt}",
            d.DisbursementId, d.ApplicationId, d.Amount,
            d.ScheduledDate, DateTime.UtcNow);
    }

    private static void ValidateStatusTransition(DisbursementStatus current, DisbursementStatus next)
    {
        // Allowed transitions:
        // Pending   → Scheduled, Cancelled
        // Scheduled → Paid, PartiallyPaid, Cancelled
        var allowed = new Dictionary<DisbursementStatus, HashSet<DisbursementStatus>>
        {
            [DisbursementStatus.Pending]   = new() { DisbursementStatus.Scheduled, DisbursementStatus.Cancelled },
            [DisbursementStatus.Scheduled] = new() { DisbursementStatus.Paid, DisbursementStatus.PartiallyPaid, DisbursementStatus.Cancelled }
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
        Status         = d.Status.ToString()  // returns "Pending", "Scheduled" etc.
    };
}
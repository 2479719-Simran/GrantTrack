using System;
using GrantTrack.Dto.DisbursementDtos;

namespace GrantTrack.Service.DisbursementServices;

public interface IDisbursementService
{
    Task<DisbursementResponseDto> CreateDisbursementAsync(CreateDisbursementDto dto);
    Task<DisbursementResponseDto?> UpdateDisbursementAsync(int id, UpdateDisbursementDto dto);
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto);

}
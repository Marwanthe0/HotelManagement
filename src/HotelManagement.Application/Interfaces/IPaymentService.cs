using HotelManagement.Application.DTOs.Payments;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponseDTO>> GetAllPaymentsAsync();
    Task<PaymentResponseDTO?> GetPaymentByIdAsync(int id);
    Task<PaymentResponseDTO> CreatePaymentAsync(CreatePaymentDTO dto);
    Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByBookingIdAsync(int bookingId);

    Task<PaymentSummaryDTO?> GetPaymentSummaryAsync(int bookingId);
}

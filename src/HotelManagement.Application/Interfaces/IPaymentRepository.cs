using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Interfaces;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment> CreateAsync(Payment payment);
    Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId);
}

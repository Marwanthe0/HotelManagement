using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Interfaces;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment> CreateAsync(Payment payment);
    Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId);

    /// <summary>
    /// Sums the amounts of all successful ("Paid") payment records for a booking.
    /// Aggregated in the database rather than in memory.
    /// </summary>
    Task<decimal> GetPaidAmountByBookingIdAsync(int bookingId);

    Task<bool> HasPaymentsForBookingAsync(int bookingId);
}



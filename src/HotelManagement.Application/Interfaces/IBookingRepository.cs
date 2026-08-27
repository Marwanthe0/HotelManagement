using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Interfaces;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(int id);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime CheckInDate, DateTime CheckOutDate,
                                    int? excludedBookingId = null);

    /// <summary>
    /// Returns the ids of rooms that are blocked by an active booking overlapping the
    /// requested dates. Uses the same overlap and blocking-status rules as
    /// <see cref="IsRoomAvailableAsync"/> so the availability logic is not duplicated.
    /// </summary>
    Task<IEnumerable<int>> GetBookedRoomIdsAsync(DateTime checkInDate, DateTime checkOutDate);

    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(Booking booking);

    Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Booking>> GetByStatusAsync(string status);
    Task<bool> HasActiveBookingsForRoomAsync(int roomId);
    Task<bool> HasBookingsForCustomerAsync(int customerId);
}


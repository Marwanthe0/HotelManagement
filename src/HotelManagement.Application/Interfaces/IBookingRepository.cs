using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Interfaces;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(int id);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime CheckInDate, DateTime CheckOutDate,
                                    int? excludedBookingId = null);

    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(Booking booking);
}
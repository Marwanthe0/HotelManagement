using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly HotelDbContext _context;

    public BookingRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        return await _context.Bookings.ToListAsync();
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        return booking;
    }

    public async Task<bool> IsRoomAvailableAsync(
        int roomId,
        DateTime checkInDate,
        DateTime checkOutDate,
        int? excludedBookingId = null
    )
    {
        var hasoverlap = await _context.Bookings.AnyAsync(b =>
            b.RoomId == roomId
            && b.CheckInDate < checkOutDate
            && b.CheckOutDate > checkInDate
            && (!excludedBookingId.HasValue || b.Id != excludedBookingId.Value)
            && b.Status != "Cancelled"
            && b.Status != "CheckedOut"
        );
        return !hasoverlap;
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Booking booking)
    {
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Bookings.Where(b => b.CustomerId == customerId).ToListAsync();
    }
}

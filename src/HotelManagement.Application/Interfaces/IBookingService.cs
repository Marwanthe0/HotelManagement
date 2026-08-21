using HotelManagement.Application.DTOs.Bookings;
using HotelManagement.Domain.Entities;
namespace HotelManagement.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingResponseDTO>> GetAllAsync();
    Task<BookingResponseDTO?> GetByIdAsync(int id);
    Task<BookingResponseDTO> CreateAsync(CreateBookingDTO dto);
    Task<BookingResponseDTO?> UpdateAsync(int id, UpdateBookingDTO dto);
    Task<bool> DeleteAsync(int id);
}
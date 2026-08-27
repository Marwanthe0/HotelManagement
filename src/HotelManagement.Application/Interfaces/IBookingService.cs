using HotelManagement.Application.DTOs.Bookings;

namespace HotelManagement.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingResponseDTO>> GetAllAsync(string? status = null);

    Task<BookingResponseDTO?> GetByIdAsync(int id);
    Task<BookingResponseDTO> CreateAsync(CreateBookingDTO dto);
    Task<BookingResponseDTO?> UpdateAsync(int id, UpdateBookingDTO dto);
    Task<bool> DeleteAsync(int id);
    Task<BookingResponseDTO?> ConfirmAsync(int id);
    Task<BookingResponseDTO?> CancelAsync(int id);
    Task<BookingResponseDTO?> CheckInAsync(int id);
    Task<BookingResponseDTO?> CheckOutAsync(int id);

    Task<IEnumerable<BookingResponseDTO>> GetByCustomerIdAsync(int customerId);

}

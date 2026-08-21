using HotelManagement.Application.DTOs;
using HotelManagement.Application.DTOs.Bookings;
using HotelManagement.Application.DTOs.Rooms;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRoomRepository _roomRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        ICustomerRepository customerRepository,
        IRoomRepository roomRepository
    )
    {
        _bookingRepository = bookingRepository;
        _customerRepository = customerRepository;
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<BookingResponseDTO>> GetAllAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();

        return bookings.Select(b => new BookingResponseDTO
        {
            Id = b.Id,
            CustomerId = b.CustomerId,
            RoomId = b.RoomId,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            BookingDate = b.BookingDate,
            Status = b.Status,
            TotalAmount = b.TotalAmount
        });
    }

    public async Task<BookingResponseDTO?> GetByIdAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking is null)
            return null;
        return new BookingResponseDTO
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            RoomId = booking.RoomId,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            BookingDate = booking.BookingDate,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount
        };
    }

    public async Task<BookingResponseDTO> CreateAsync(CreateBookingDTO dto)
    {
        // 1. Validate dates
        if (dto.CheckInDate >= dto.CheckOutDate)
        {
            throw new InvalidOperationException("Check-out Date must be after check-in date.");
        }

        // 2. Check Customer
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);

        if (customer is null)
        {
            throw new InvalidOperationException("Customer with this Id not found.");
        }

        // 3. Check Room
        var room = await _roomRepository.GetByIdAsync(dto.RoomId);

        if (room is null)
        {
            throw new InvalidOperationException("Room with this Id not found.");
        }

        // 4. Check room availability
        var isAvailable = await _bookingRepository.IsRoomAvailableAsync(
            dto.RoomId,
            dto.CheckInDate,
            dto.CheckOutDate
        );

        if (!isAvailable)
        {
            throw new InvalidOperationException(
                "The room is not available for the selected dates."
            );
        }

        // 5. Caculating Payments
        var numberOfNights = (dto.CheckOutDate - dto.CheckInDate).Days;
        var totalAmount = numberOfNights * room.PricePerNight;

        var booking = new Booking
        {
            CustomerId = dto.CustomerId,
            RoomId = dto.RoomId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            BookingDate = DateTime.UtcNow,
            Status = "Pending",
            TotalAmount = totalAmount,
        };
        await _bookingRepository.AddAsync(booking);

        return new BookingResponseDTO
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            RoomId = booking.RoomId,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            BookingDate = booking.BookingDate,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
        };
    }

    public async Task<BookingResponseDTO?> UpdateAsync(int id, UpdateBookingDTO dto)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking is null)
            return null;

        if (dto.CheckInDate >= dto.CheckOutDate)
            throw new InvalidOperationException("Check-out date must be after check-in date.");

        var room = await _roomRepository.GetByIdAsync(dto.RoomId);

        if (room is null)
            throw new InvalidOperationException("Room Not Found.");

        var isAvailable = await _bookingRepository.IsRoomAvailableAsync(
            dto.RoomId,
            dto.CheckInDate,
            dto.CheckOutDate,
            id
        );
        if (!isAvailable)
            throw new InvalidOperationException(
                "The Room is not available for the selected dates."
            );

        var numberOfNights = (dto.CheckOutDate - dto.CheckInDate).Days;

        booking.RoomId = dto.RoomId;
        booking.CheckInDate = dto.CheckInDate;
        booking.CheckOutDate = dto.CheckOutDate;
        booking.TotalAmount = numberOfNights * room.PricePerNight;

        await _bookingRepository.UpdateAsync(booking);

        return new BookingResponseDTO
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            RoomId = booking.RoomId,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            BookingDate = booking.BookingDate,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking is null)
            return false;

        await _bookingRepository.DeleteAsync(booking);
        return true;
    }
}

using HotelManagement.Application.DTOs.Bookings;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IPaymentRepository _paymentRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        ICustomerRepository customerRepository,
        IRoomRepository roomRepository,
        IPaymentRepository paymentRepository
    )
    {
        _bookingRepository = bookingRepository;
        _customerRepository = customerRepository;
        _roomRepository = roomRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<IEnumerable<BookingResponseDTO>> GetAllAsync(string? status = null)
    {
        var bookings = string.IsNullOrWhiteSpace(status)
            ? await _bookingRepository.GetAllAsync()
            : await _bookingRepository.GetByStatusAsync(NormalizeStatus(status));

        return bookings.Select(b => MapToResponseDTO(b));
    }


    public async Task<BookingResponseDTO?> GetByIdAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking is null)
            return null;

        return MapToResponseDTO(booking);
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

        // 3b. Room must be available (not under maintenance)
        if (!room.IsAvailable)
        {
            throw new InvalidOperationException(
                "This room is currently under maintenance and cannot be booked.");
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

        // 5. Calculating Total Amount (server-side, never trust the client)
        var totalAmount = CalculateTotalAmount(
            dto.CheckInDate,
            dto.CheckOutDate,
            room.PricePerNight
        );

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

        return MapToResponseDTO(booking);
    }

    public async Task<BookingResponseDTO?> UpdateAsync(int id, UpdateBookingDTO dto)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking is null)
            return null;

        // A booking may only be rescheduled while it is still Pending or Confirmed.
        // Changing dates/room after check-in, check-out or cancellation is not meaningful.
        if (booking.Status != "Pending" && booking.Status != "Confirmed")
        {
            throw new InvalidOperationException(
                $"A booking with status '{booking.Status}' can no longer be updated."
            );
        }

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

        // Recalculate the total from the room price and nights (never trust the client).
        var newTotalAmount = CalculateTotalAmount(
            dto.CheckInDate,
            dto.CheckOutDate,
            room.PricePerNight
        );

        // Payments are immutable, so the new total must never drop below what is already paid.
        var paidAmount = await _paymentRepository.GetPaidAmountByBookingIdAsync(id);
        if (newTotalAmount < paidAmount)
        {
            throw new InvalidOperationException(
                $"The new total amount ({newTotalAmount}) cannot be less than the amount "
                    + $"already paid ({paidAmount}) for this booking."
            );
        }

        booking.RoomId = dto.RoomId;
        booking.CheckInDate = dto.CheckInDate;
        booking.CheckOutDate = dto.CheckOutDate;
        booking.TotalAmount = newTotalAmount;

        await _bookingRepository.UpdateAsync(booking);

        return MapToResponseDTO(booking);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking is null)
            return false;

        // Payments use DeleteBehavior.Restrict and represent financial history, so a booking
        // that already has payment records must not be deleted. Cancel it instead.
        var hasPayments = await _paymentRepository.HasPaymentsForBookingAsync(id);
        if (hasPayments)
        {
            throw new InvalidOperationException(
                "Cannot delete a booking that has payment records. Cancel the booking instead "
                    + "so the financial history is preserved."
            );
        }

        await _bookingRepository.DeleteAsync(booking);
        return true;
    }


    public async Task<BookingResponseDTO?> ConfirmAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking is null)
            return null;
        if (booking.Status != "Pending")
        {
            throw new InvalidOperationException("Only pending bookings can be confirmed.");
        }
        booking.Status = "Confirmed";

        await _bookingRepository.UpdateAsync(booking);

        return MapToResponseDTO(booking);
    }

    public async Task<BookingResponseDTO?> CancelAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking is null)
            return null;

        if (booking.Status != "Pending" && booking.Status != "Confirmed")
        {
            throw new InvalidOperationException(
                "Only pending or confirmed bookings can be cancelled."
            );
        }

        booking.Status = "Cancelled";

        await _bookingRepository.UpdateAsync(booking);

        return MapToResponseDTO(booking);
    }

    public async Task<BookingResponseDTO?> CheckInAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking is null)
            return null;

        if (booking.Status != "Confirmed")
        {
            throw new InvalidOperationException("Only Confirmed bookings can be checked in.");
        }

        booking.Status = "CheckedIn";
        await _bookingRepository.UpdateAsync(booking);

        return MapToResponseDTO(booking);
    }

    public async Task<BookingResponseDTO?> CheckOutAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking is null)
            return null;

        if (booking.Status != "CheckedIn")
        {
            throw new InvalidOperationException("Only checked-in bookings can be checked out.");
        }

        // Check if payment is fully completed before checkout
        var paidAmount = await _paymentRepository.GetPaidAmountByBookingIdAsync(id);
        var remainingAmount = booking.TotalAmount - paidAmount;


        if (remainingAmount > 0)
        {
            throw new InvalidOperationException(
                $"Payment is not fully completed. Remaining amount: {remainingAmount}."
            );
        }

        booking.Status = "CheckedOut";
        await _bookingRepository.UpdateAsync(booking);

        return MapToResponseDTO(booking);
    }

    public async Task<IEnumerable<BookingResponseDTO>> GetByCustomerIdAsync(int customerId)
    {
        var bookings = await _bookingRepository.GetByCustomerIdAsync(customerId);

        return bookings.Select(b => MapToResponseDTO(b));
    }

    /// <summary>
    /// Calculates the booking total as numberOfNights * pricePerNight.
    /// Only the date component matters because check-in/check-out are business dates,
    /// so a stray time component cannot silently reduce the number of billed nights.
    /// </summary>
    private static decimal CalculateTotalAmount(
        DateTime checkInDate,
        DateTime checkOutDate,
        decimal pricePerNight
    )
    {
        var numberOfNights = (checkOutDate.Date - checkInDate.Date).Days;

        // Same-day bookings with a later check-out time still count as one night.
        if (numberOfNights <= 0)
            numberOfNights = 1;

        return numberOfNights * pricePerNight;
    }

    /// <summary>
    /// Maps an incoming status filter onto the exact status string used by the project,
    /// so that "confirmed", "CONFIRMED" and "checkedout" all work from a query string.
    /// Unknown values are passed through and simply return no results.
    /// </summary>
    private static string NormalizeStatus(string status)
    {
        return status.Trim().ToLowerInvariant() switch
        {
            "pending" => "Pending",
            "confirmed" => "Confirmed",
            "cancelled" or "canceled" => "Cancelled",
            "checkedin" or "checked-in" => "CheckedIn",
            "checkedout" or "checked-out" => "CheckedOut",
            _ => status.Trim(),
        };
    }

    private static BookingResponseDTO MapToResponseDTO(Booking booking)

    {
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
}

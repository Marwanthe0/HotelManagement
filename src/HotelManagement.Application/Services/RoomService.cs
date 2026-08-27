using HotelManagement.Application.DTOs.Rooms;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;

    public RoomService(IRoomRepository roomRepository, IBookingRepository bookingRepository)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<IEnumerable<RoomResponseDto>> GetAllRoomsAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        return rooms.Select(room => MapToResponseDto(room));
    }

    public async Task<RoomResponseDto?> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null) return null;

        return MapToResponseDto(room);
    }



    public async Task<RoomResponseDto> AddRoomAsync(CreateRoomDTO dto)
    {
        var exists = await _roomRepository.ExistsByRoomNumberAsync(dto.RoomNumber);
        if (exists)
        {
            throw new InvalidOperationException($"Room Number {dto.RoomNumber} already exists.");
        }
        var room = new Room
        {
            RoomNumber = dto.RoomNumber,
            RoomType = dto.RoomType,
            PricePerNight = dto.PricePerNight,
            IsAvailable = dto.IsAvailable
        };

        await _roomRepository.AddAsync(room);

        return MapToResponseDto(room);
    }



    public async Task<RoomResponseDto?> UpdateRoomAsync(int id, UpdateRoomDto dto)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null)
        {
            return null;
        }

        var exists = await _roomRepository
        .ExistsByRoomNumberAsync(dto.RoomNumber, id);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Room number {dto.RoomNumber} already exists.");
        }

        room.RoomNumber = dto.RoomNumber;
        room.RoomType = dto.RoomType;
        room.PricePerNight = dto.PricePerNight;
        room.IsAvailable = dto.IsAvailable;

        await _roomRepository.UpdateAsync(room);

        return MapToResponseDto(room);
    }


    public async Task<bool> DeleteRoomAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null)
        {
            return false;
        }

        // Prevent deletion if room has active bookings
        var hasActiveBookings = await _bookingRepository.HasActiveBookingsForRoomAsync(id);
        if (hasActiveBookings)
        {
            throw new InvalidOperationException(
                "Cannot delete room because it has active bookings (Pending, Confirmed, or CheckedIn).");
        }

        await _roomRepository.DeleteAsync(room);
        return true;
    }

    public async Task<IEnumerable<RoomResponseDto>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate)
    {
        if (checkInDate == default || checkOutDate == default)
        {
            throw new ArgumentException(
                "Both checkInDate and checkOutDate query parameters are required.");
        }

        if (checkInDate >= checkOutDate)
        {
            throw new InvalidOperationException("Check-out date must be after check-in date.");
        }

        // Resolve the blocked rooms in one query instead of querying per room,
        // reusing the repository's overlap/blocking-status rules.
        var rooms = await _roomRepository.GetAllAsync();
        var bookedRoomIds = await _bookingRepository.GetBookedRoomIdsAsync(
            checkInDate, checkOutDate);

        var bookedRoomIdSet = bookedRoomIds.ToHashSet();

        return rooms
            .Where(room => !bookedRoomIdSet.Contains(room.Id))
            .Select(room => MapToResponseDto(room))
            .ToList();
    }

    private static RoomResponseDto MapToResponseDto(Room room)
    {
        return new RoomResponseDto
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            PricePerNight = room.PricePerNight,
            IsAvailable = room.IsAvailable
        };
    }
}


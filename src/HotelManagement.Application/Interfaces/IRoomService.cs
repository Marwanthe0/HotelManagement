using HotelManagement.Application.DTOs.Rooms;

namespace HotelManagement.Application.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomResponseDto>> GetAllRoomsAsync();

    Task<RoomResponseDto?> GetRoomByIdAsync(int id);

    Task<RoomResponseDto> AddRoomAsync(CreateRoomDTO dto);

    Task<RoomResponseDto?> UpdateRoomAsync(int id, UpdateRoomDto dto);

    Task<bool> DeleteRoomAsync(int id);

    Task<IEnumerable<RoomResponseDto>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate);
}
using HotelManagement.Application.DTOs.Rooms;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<RoomResponseDto>> GetAllRoomsAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        return rooms.Select(room => new RoomResponseDto
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            PricePerNight = room.PricePerNight,
            IsAvailable = room.IsAvailable
        });
    }

    public async Task<RoomResponseDto?> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null) return null;
        return new RoomResponseDto
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            PricePerNight = room.PricePerNight,
            IsAvailable = room.IsAvailable
        };
    }


    public async Task AddRoomAsync(CreateRoomDTO dto)
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
    }


    public async Task UpdateRoomAsync(int id,UpdateRoomDto dto)
    {
        var exists = await _roomRepository
        .ExistsByRoomNumberAsync(dto.RoomNumber, id);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Room number {dto.RoomNumber} already exists.");
        }
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null)
        {
            return;
        }
        
        room.RoomNumber = dto.RoomNumber;
        room.RoomType = dto.RoomType;
        room.PricePerNight = dto.PricePerNight;
        room.IsAvailable = dto.IsAvailable;
        
        await _roomRepository.UpdateAsync(room);
    }

    public async Task DeleteRoomAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if(room != null)
        {
            await _roomRepository.DeleteAsync(room);
        }
    }
}
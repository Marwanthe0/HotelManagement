using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Interfaces;

public interface IRoomRepository
{
    //Enumerable interface for getting list of rooms
    Task<IEnumerable<Room>> GetAllAsync();
    Task<Room?> GetByIdAsync(int id);
    Task<bool> ExistsByRoomNumberAsync(string roomNumber,int?excludeRoomId = null);
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(Room room);
}
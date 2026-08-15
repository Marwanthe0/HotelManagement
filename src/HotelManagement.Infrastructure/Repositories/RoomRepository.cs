using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Repositories;


public class RoomRepository: IRoomRepository
{
    private readonly HotelDbContext _context;
    public RoomRepository(HotelDbContext context)
    {
        _context = context;
    }

    //GET All
    public async Task<IEnumerable<Room>> GetAllAsync()
    {
        return await _context.Rooms.ToListAsync();
    }

    //GET One by id
    public async Task<Room?> GetByIdAsync(int id)
    {
        return await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
    }
    public async Task<bool> ExistsByRoomNumberAsync(string roomNumber,int?excludeRoomId = null)
    {
    return await _context.Rooms
        .AnyAsync(r => r.RoomNumber == roomNumber && 
        (!excludeRoomId.HasValue || r.Id != excludeRoomId.Value));
    }

    //POST 
    public async Task AddAsync(Room room)
    {
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
    }

    //PUT
    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Room room)
    {
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }


}
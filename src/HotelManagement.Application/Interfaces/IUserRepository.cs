using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username, int? excludeUserId = null);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

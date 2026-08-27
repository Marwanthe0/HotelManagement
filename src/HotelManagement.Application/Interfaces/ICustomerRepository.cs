using HotelManagement.Domain.Entities;
namespace HotelManagement.Application.Interfaces;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<IEnumerable<Customer>> SearchAsync(string search);
    Task<Customer?> GetByIdAsync(int id);
    Task<bool> ExistsByEmailAsync(string email, int? excludeCustomerId = null);

    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
}
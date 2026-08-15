using HotelManagement.Domain.Entities;
namespace HotelManagement.Application.Interfaces;

public interface ICustomerReository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync();
    Task<bool> ExistsByEmailAsynce(string email);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
}
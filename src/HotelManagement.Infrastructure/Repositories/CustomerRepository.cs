using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly HotelDbContext _context;

    public CustomerRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers.ToListAsync();
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string search)
    {
        return await _context.Customers
            .Where(c =>
                c.FirstName.Contains(search)
                || c.LastName.Contains(search)
                || c.Email.Contains(search)
                || c.Phone.Contains(search))
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeCustomerId = null)
    {
        return await _context.Customers
            .AnyAsync(c => c.Email == email
                && (!excludeCustomerId.HasValue || c.Id != excludeCustomerId.Value));
    }


    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }
}
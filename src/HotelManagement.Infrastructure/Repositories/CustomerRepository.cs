using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Repositories;

public class CustomerRepository: ICustomerReository
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

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistByEmailAsync(string email)
    {
        return await _context.Customers.AnyAsync(c => c.Email == email);
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsynce(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Customer customer)
    {
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    public Task<Customer?> GetByIdAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsByEmailAsynce(string email)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Customer customer)
    {
        throw new NotImplementedException();
    }
}

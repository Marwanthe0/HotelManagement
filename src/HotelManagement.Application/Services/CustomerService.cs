using HotelManagement.Application.DTOs.Customers;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class CustomerService:ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    
    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<CustomerResponseDTO>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();

        return customers.Select(c=>new CustomerResponseDTO
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address
        });
    }

    public async Task<CustomerResponseDTO?> GetByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if(customer is null) return null;
        
        return new CustomerResponseDTO
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address
        };
    }

    public async Task<CustomerResponseDTO> CreateAsync(CreateCustomerDTO dto)
    {
        var emailExists = await _customerRepository.ExistsByEmailAsync(dto.Email);
        if(emailExists)
        {
            throw new InvalidOperationException("A customer with this email already exists.");
        }
        var customer = new Customer 
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address
        };
        
        await _customerRepository.AddAsync(customer);
        return new CustomerResponseDTO
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address     
        };
    }

    public async Task<CustomerResponseDTO?> UpdateAsync(int id,UpdateCustomerDTO dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if(customer is null) return null;
        
        if(customer.Email != dto.Email)
        {
            var emailExists = await _customerRepository.ExistsByEmailAsync(dto.Email);
            if(emailExists)
            {
                throw new InvalidOperationException("A customer with this email already exists.");
            }
        }
        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        await _customerRepository.UpdateAsync(customer);
        return new CustomerResponseDTO
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address
        };
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if(customer is null) return false;
        await _customerRepository.DeleteAsync(customer);
        return true;
    }
}
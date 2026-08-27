using HotelManagement.Application.DTOs.Employees;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<IEnumerable<EmployeeResponseDTO>> GetAllAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();

        return employees.Select(e => new EmployeeResponseDTO
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            Role = e.Role,
            Salary = e.Salary
        });
    }

    public async Task<EmployeeResponseDTO?> GetByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);

        if (employee is null) return null;

        return new EmployeeResponseDTO
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Role = employee.Role,
            Salary = employee.Salary
        };
    }

    public async Task<EmployeeResponseDTO> CreateAsync(CreateEmployeeDTO dto)
    {
        var emailExists = await _employeeRepository.ExistsByEmailAsync(dto.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("An employee with this email already exists.");
        }

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = dto.Role,
            Salary = dto.Salary
        };

        await _employeeRepository.AddAsync(employee);

        return new EmployeeResponseDTO
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Role = employee.Role,
            Salary = employee.Salary
        };
    }

    public async Task<EmployeeResponseDTO?> UpdateAsync(int id, UpdateEmployeeDTO dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee is null) return null;

        if (employee.Email != dto.Email)
        {
            var emailExists = await _employeeRepository.ExistsByEmailAsync(dto.Email);
            if (emailExists)
            {
                throw new InvalidOperationException("An employee with this email already exists.");
            }
        }

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.Role = dto.Role;
        employee.Salary = dto.Salary;

        await _employeeRepository.UpdateAsync(employee);

        return new EmployeeResponseDTO
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Role = employee.Role,
            Salary = employee.Salary
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee is null) return false;
        await _employeeRepository.DeleteAsync(employee);
        return true;
    }
}

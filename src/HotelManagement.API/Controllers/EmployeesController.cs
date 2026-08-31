using HotelManagement.Application.DTOs.Employees;
using HotelManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    //GET /api/employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeResponseDTO>>> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    //GET /api/employees/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeResponseDTO>> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
            return NotFound();

        return Ok(employee);
    }

    //POST /api/employees
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<EmployeeResponseDTO>> Create(CreateEmployeeDTO dto)
    {
        var employee = await _employeeService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    //PUT /api/employees/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmployeeResponseDTO>> Update(int id, UpdateEmployeeDTO dto)
    {
        var employee = await _employeeService.UpdateAsync(id, dto);
        if (employee is null)
            return NotFound();

        return Ok(employee);
    }

    //DELETE /api/employees/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _employeeService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.DTOs.Employees;

public class CreateEmployeeDTO
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty;

    [Range(0, 10000000)]
    public decimal Salary { get; set; }
}

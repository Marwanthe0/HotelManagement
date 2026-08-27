using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.DTOs.Customers;

public class CreateCustomerDTO
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

    [StringLength(200)]
    public string Address { get; set; } = string.Empty;
}
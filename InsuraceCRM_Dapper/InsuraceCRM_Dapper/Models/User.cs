using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Mobile number")]
    public string? Mobile { get; set; }

    [Required, StringLength(50)]
    public string Role { get; set; } = "Employee";

    public bool IsActive { get; set; } = true;
}

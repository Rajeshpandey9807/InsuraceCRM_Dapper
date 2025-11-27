using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class User
{
    public int CustomerID { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Mobile { get; set; }
    public string RoleId { get; set; }

    [Required, StringLength(50)]
    public string Role { get; set; } = "Employee";
}

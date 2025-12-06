using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.ViewModels;

public class UserFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Mobile number")]
    public string? Mobile { get; set; }

    [Required]
    public string Role { get; set; } = "Employee";

    public int RoleId { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Id.HasValue)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult("Password is required for new users.", new[] { nameof(Password) });
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                yield return new ValidationResult("Confirm password is required for new users.", new[] { nameof(ConfirmPassword) });
            }
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class Role
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public bool IsSystem { get; set; }
}

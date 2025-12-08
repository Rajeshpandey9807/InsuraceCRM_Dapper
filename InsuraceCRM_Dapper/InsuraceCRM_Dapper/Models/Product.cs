using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required, StringLength(50)]
    public string CommissionType { get; set; } = "Percentage";

    [Range(typeof(decimal), "0.00", "1000000")]
    public decimal CommissionValue { get; set; }

    [StringLength(500)]
    public string? CommissionNotes { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedOn { get; set; }

    public List<ProductDocument> Documents { get; set; } = new();
}

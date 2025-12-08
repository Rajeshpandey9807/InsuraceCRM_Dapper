using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InsuraceCRM_Dapper.Models;
using Microsoft.AspNetCore.Http;

namespace InsuraceCRM_Dapper.ViewModels;

public class ProductFormViewModel
{
    public int? Id { get; set; }

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

    public List<ProductDocument> ExistingDocuments { get; set; } = new();

    public List<IFormFile>? NewDocuments { get; set; }

    public bool IsEdit => Id.HasValue;

    public Product ToProduct() => new()
    {
        Id = Id ?? 0,
        Name = Name.Trim(),
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        CommissionType = CommissionType.Trim(),
        CommissionValue = CommissionValue,
        CommissionNotes = string.IsNullOrWhiteSpace(CommissionNotes) ? null : CommissionNotes.Trim()
    };

    public static ProductFormViewModel FromProduct(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        CommissionType = product.CommissionType,
        CommissionValue = product.CommissionValue,
        CommissionNotes = product.CommissionNotes,
        ExistingDocuments = product.Documents
    };
}

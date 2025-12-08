using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class ProductDocument
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required, StringLength(300)]
    public string FileName { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ContentType { get; set; } = "application/octet-stream";

    [Required, StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }
    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
}

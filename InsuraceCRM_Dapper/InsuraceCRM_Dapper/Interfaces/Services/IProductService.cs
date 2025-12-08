using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<int> CreateAsync(Product product, IEnumerable<ProductDocument> documents);
    Task UpdateAsync(Product product, IEnumerable<ProductDocument> newDocuments);
    Task DeleteAsync(int id);
    Task<ProductDocument?> GetDocumentAsync(int documentId);
    Task DeleteDocumentAsync(int documentId);
}

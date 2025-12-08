using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Repositories;

public interface IProductRepository
{
    Task<int> InsertAsync(Product product, IEnumerable<ProductDocument> documents);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddDocumentsAsync(int productId, IEnumerable<ProductDocument> documents);
    Task<ProductDocument?> GetDocumentByIdAsync(int documentId);
    Task DeleteDocumentAsync(int documentId);
}

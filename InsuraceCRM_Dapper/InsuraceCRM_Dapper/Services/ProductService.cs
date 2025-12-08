using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<IEnumerable<Product>> GetAllAsync() => _productRepository.GetAllAsync();

    public Task<Product?> GetByIdAsync(int id) => _productRepository.GetByIdAsync(id);

    public Task<ProductDocument?> GetDocumentAsync(int documentId) =>
        _productRepository.GetDocumentByIdAsync(documentId);

    public async Task<int> CreateAsync(Product product, IEnumerable<ProductDocument> documents)
    {
        product.CreatedOn = DateTime.UtcNow;
        product.UpdatedOn = product.CreatedOn;
        return await _productRepository.InsertAsync(product, documents);
    }

    public async Task UpdateAsync(Product product, IEnumerable<ProductDocument> newDocuments)
    {
        product.UpdatedOn = DateTime.UtcNow;
        await _productRepository.UpdateAsync(product);
        await _productRepository.AddDocumentsAsync(product.Id, newDocuments);
    }

    public Task DeleteAsync(int id) => _productRepository.DeleteAsync(id);

    public Task DeleteDocumentAsync(int documentId) => _productRepository.DeleteDocumentAsync(documentId);
}

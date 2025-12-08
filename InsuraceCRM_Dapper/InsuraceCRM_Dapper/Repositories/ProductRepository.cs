using System.Data;
using System.Linq;
using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> InsertAsync(Product product, IEnumerable<ProductDocument> documents)
    {
        const string insertProductSql = @"
            INSERT INTO Products (
                Name,
                Description,
                CommissionType,
                CommissionValue,
                CommissionNotes,
                CreatedOn,
                UpdatedOn)
            VALUES (
                @Name,
                @Description,
                @CommissionType,
                @CommissionValue,
                @CommissionNotes,
                @CreatedOn,
                @UpdatedOn);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        const string insertDocumentSql = @"
            INSERT INTO ProductDocuments (
                ProductId,
                FileName,
                OriginalFileName,
                ContentType,
                FilePath,
                FileSize,
                UploadedOn)
            VALUES (
                @ProductId,
                @FileName,
                @OriginalFileName,
                @ContentType,
                @FilePath,
                @FileSize,
                @UploadedOn);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var productId = await connection.ExecuteScalarAsync<int>(insertProductSql, product, transaction);

            if (documents?.Any() == true)
            {
                foreach (var document in documents)
                {
                    document.ProductId = productId;
                    await connection.ExecuteAsync(insertDocumentSql, document, transaction);
                }
            }

            transaction.Commit();
            return productId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateAsync(Product product)
    {
        const string sql = @"
            UPDATE Products
            SET Name = @Name,
                Description = @Description,
                CommissionType = @CommissionType,
                CommissionValue = @CommissionValue,
                CommissionNotes = @CommissionNotes,
                UpdatedOn = @UpdatedOn
            WHERE Id = @Id;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, product);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Products WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        const string productSql = "SELECT * FROM Products WHERE Id = @Id;";
        const string documentsSql = "SELECT * FROM ProductDocuments WHERE ProductId = @Id;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var multi = await connection.QueryMultipleAsync($"{productSql} {documentsSql}", new { Id = id });

        var product = await multi.ReadSingleOrDefaultAsync<Product>();
        if (product is null)
        {
            return null;
        }

        product.Documents = (await multi.ReadAsync<ProductDocument>()).ToList();
        return product;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        const string productsSql = "SELECT * FROM Products ORDER BY Name;";
        const string documentsSql = "SELECT * FROM ProductDocuments WHERE ProductId IN @Ids;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var products = (await connection.QueryAsync<Product>(productsSql)).ToList();

        if (products.Count == 0)
        {
            return products;
        }

        var documents = (await connection.QueryAsync<ProductDocument>(documentsSql, new
        {
            Ids = products.Select(p => p.Id)
        })).ToList();

        var documentLookup = documents.ToLookup(d => d.ProductId);

        foreach (var product in products)
        {
            product.Documents = documentLookup[product.Id].ToList();
        }

        return products;
    }

    public async Task AddDocumentsAsync(int productId, IEnumerable<ProductDocument> documents)
    {
        const string sql = @"
            INSERT INTO ProductDocuments (
                ProductId,
                FileName,
                OriginalFileName,
                ContentType,
                FilePath,
                FileSize,
                UploadedOn)
            VALUES (
                @ProductId,
                @FileName,
                @OriginalFileName,
                @ContentType,
                @FilePath,
                @FileSize,
                @UploadedOn);";

        if (documents?.Any() != true)
        {
            return;
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        foreach (var document in documents)
        {
            document.ProductId = productId;
            await connection.ExecuteAsync(sql, document);
        }
    }

    public async Task<ProductDocument?> GetDocumentByIdAsync(int documentId)
    {
        const string sql = "SELECT * FROM ProductDocuments WHERE Id = @DocumentId;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<ProductDocument>(sql, new { DocumentId = documentId });
    }

    public async Task DeleteDocumentAsync(int documentId)
    {
        const string sql = "DELETE FROM ProductDocuments WHERE Id = @DocumentId;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { DocumentId = documentId });
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize]
public class ProductController : Controller
{
    private static readonly string[] AllowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductService productService,
        IWebHostEnvironment environment,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        var viewModel = new ProductListViewModel
        {
            Products = products,
            CanManage = User.IsInRole("Admin") || User.IsInRole("Manager")
        };

        return View(viewModel);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProductFormViewModel());
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel viewModel)
    {
        ValidateDocuments(viewModel.NewDocuments);

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var product = viewModel.ToProduct();
        var documents = await SaveDocumentsAsync(viewModel.NewDocuments);

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        await _productService.CreateAsync(product, documents);
        TempData["ProductSuccess"] = "Product created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var viewModel = ProductFormViewModel.FromProduct(product);
        return View(viewModel);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel viewModel)
    {
        if (!viewModel.Id.HasValue)
        {
            return BadRequest();
        }

        ValidateDocuments(viewModel.NewDocuments);

        if (!ModelState.IsValid)
        {
            var currentProduct = await _productService.GetByIdAsync(viewModel.Id.Value);
            viewModel.ExistingDocuments = currentProduct?.Documents ?? new List<ProductDocument>();
            return View(viewModel);
        }

        var product = viewModel.ToProduct();
        var documents = await SaveDocumentsAsync(viewModel.NewDocuments);

        if (!ModelState.IsValid)
        {
            var currentProduct = await _productService.GetByIdAsync(viewModel.Id.Value);
            viewModel.ExistingDocuments = currentProduct?.Documents ?? new List<ProductDocument>();
            return View(viewModel);
        }

        await _productService.UpdateAsync(product, documents);
        TempData["ProductSuccess"] = "Product updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        await _productService.DeleteAsync(id);
        DeleteFiles(product.Documents);

        TempData["ProductSuccess"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> PreviewDocument(int documentId)
    {
        var document = await _productService.GetDocumentAsync(documentId);
        if (document is null)
        {
            return NotFound();
        }

        var physicalPath = GetPhysicalFilePath(document.FilePath);
        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound();
        }

        var stream = System.IO.File.OpenRead(physicalPath);
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{document.OriginalFileName}\"";
        return File(stream, document.ContentType);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(int documentId, int productId)
    {
        var document = await _productService.GetDocumentAsync(documentId);
        if (document is null)
        {
            return NotFound();
        }

        if (document.ProductId != productId)
        {
            return BadRequest();
        }

        await _productService.DeleteDocumentAsync(documentId);
        DeleteFiles(new[] { document });

        TempData["ProductSuccess"] = "Document removed successfully.";
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    private void ValidateDocuments(IEnumerable<IFormFile>? files)
    {
        if (files is null)
        {
            return;
        }

        foreach (var file in files)
        {
            if (file is null || file.Length == 0)
            {
                ModelState.AddModelError(nameof(ProductFormViewModel.NewDocuments), "Document cannot be empty.");
                continue;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                ModelState.AddModelError(nameof(ProductFormViewModel.NewDocuments), $"Document '{file.FileName}' exceeds 20 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(ProductFormViewModel.NewDocuments), $"Document '{file.FileName}' has an unsupported file type.");
            }
        }
    }

    private async Task<List<ProductDocument>> SaveDocumentsAsync(IEnumerable<IFormFile>? files)
    {
        var documents = new List<ProductDocument>();
        if (files is null)
        {
            return documents;
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsFolder);

        foreach (var file in files.Where(f => f is not null && f.Length > 0))
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var storedFileName = $"{Guid.NewGuid():N}{extension}";
                var physicalPath = Path.Combine(uploadsFolder, storedFileName);

                await using var stream = System.IO.File.Create(physicalPath);
                await file.CopyToAsync(stream);

                documents.Add(new ProductDocument
                {
                    FileName = storedFileName,
                    OriginalFileName = file.FileName,
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                    FilePath = Path.Combine("uploads", "products", storedFileName).Replace("\\", "/"),
                    FileSize = file.Length,
                    UploadedOn = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to store document {DocumentName}", file.FileName);
                ModelState.AddModelError(nameof(ProductFormViewModel.NewDocuments), $"Unable to store '{file.FileName}'. Please try again.");
            }
        }

        if (!ModelState.IsValid && documents.Any())
        {
            // Clean up already written files when validation failed later.
            DeleteFiles(documents);
            documents.Clear();
        }

        return documents;
    }

    private void DeleteFiles(IEnumerable<ProductDocument>? documents)
    {
        if (documents is null)
        {
            return;
        }

        foreach (var document in documents)
        {
            try
            {
                var physicalPath = GetPhysicalFilePath(document.FilePath);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to delete document {Document}", document.FilePath);
            }
        }
    }

    private string GetPhysicalFilePath(string relativePath)
    {
        var sanitized = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_environment.WebRootPath, sanitized);
    }
}

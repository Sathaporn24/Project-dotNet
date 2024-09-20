using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonolithAPI.DTOs.Reponse;
using MonolithAPI.DTOs.Request;
using MonolithAPI.Models;

namespace MonolithAPI.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IConfigurationSection _imageSettings;
    private readonly AppDbContext _appDbContext;

    public ProductsController(IConfiguration configuration, AppDbContext appDbContext)
    {
        _imageSettings = configuration.GetSection("ImageSettings");
        _appDbContext = appDbContext;
    }

    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(PagingDTO<ProductDTO>))]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductDTO request)
    {
        var pageIndex = request.PageIndex;
        var pageSize = request.PageSize;
        var query = _appDbContext.Products.AsQueryable();

        if (request.OnlyMyItem)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            query = query.Where(x => x.CreatedBy == userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(x => x.Name.Contains(request.Keyword));
        }

        var itemCount = await query.CountAsync();

        var items = await query.Include(p => p.Owner).OrderByDescending(x => x.CreatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(x => new ProductDTO
        {
            Id = x.Id,
            Name = x.Name,
            Price = x.Price,
            ImagePath = string.IsNullOrEmpty(x.ImagePath) ? x.ImagePath : $"/{x.ImagePath}",
            OwnerName = x.Owner!.FullName
        }).ToListAsync();

        return Ok(new PagingDTO<ProductDTO>
        {
            Items = items,
            TotalItems = itemCount,
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ProductDetailDTO))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var curProduct = await _appDbContext.Products.Include(p => p.Owner).FirstOrDefaultAsync(x => x.Id == id);
        if (curProduct == null)
        {
            return NotFound();
        }
        var result = new ProductDetailDTO
        {
            Id = curProduct.Id,
            Name = curProduct.Name,
            Price = curProduct.Price,
            Description = curProduct.Description,
            ImagePath = string.IsNullOrEmpty(curProduct.ImagePath) ? curProduct.ImagePath : $"/{curProduct.ImagePath}",
            OwnerName = curProduct.Owner?.FullName,
        };
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(ProductDetailDTO))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromForm] CreateProductDTO request)
    {
        string? filePath = null;
        // check user upload file or not
        if (request.Image != null)
        {
            var uploadResult = await TryToUploadFileAsync(request.Image);
            if (uploadResult.Errors != null)
            {
                return BadRequest(uploadResult.Errors);
            }
            filePath = uploadResult.FilePath;
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var newProduct = new ProductModel
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            ImagePath = filePath,
            CreatedBy = userId,
            CreatedTime = DateTime.UtcNow
        };
        _appDbContext.Products.Add(newProduct);
        await _appDbContext.SaveChangesAsync();
        var ownerName = User.FindFirstValue("name");
        var result = new ProductDetailDTO
        {
            Id = newProduct.Id,
            Name = newProduct.Name,
            Price = newProduct.Price,
            Description = newProduct.Description,
            ImagePath = string.IsNullOrEmpty(filePath) ? filePath : $"/{filePath}",
            OwnerName = ownerName
        };
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditProduct(Guid id, [FromForm] UpdateProductDTO request)
    {
        string? filePath = null;
        // check user upload file or not
        if (request.Image != null)
        {
            var uploadResult = await TryToUploadFileAsync(request.Image);
            if (uploadResult.Errors != null)
            {
                return BadRequest(uploadResult.Errors);
            }
            filePath = uploadResult.FilePath;
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var curProduct = await _appDbContext.Products.FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == userId);
        if (curProduct == null)
        {
            return NotFound();
        }

        curProduct.Name = request.Name;
        curProduct.Price = request.Price;
        curProduct.Description = request.Description;
        curProduct.UpdatedBy = userId;
        curProduct.UpdatedTime = DateTime.UtcNow;

        string? imagePath = null;
        if (!string.IsNullOrEmpty(filePath))
        {
            imagePath = curProduct.ImagePath; // old image
            curProduct.ImagePath = filePath; // new image
        }

        _appDbContext.Products.Update(curProduct);
        await _appDbContext.SaveChangesAsync();

        if (!string.IsNullOrEmpty(imagePath))
        {
            // delete old image
            TryToDeleteFile(imagePath);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var curProduct = await _appDbContext.Products.FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == userId);
        if (curProduct == null)
        {
            return NotFound();
        }
        _appDbContext.Products.Remove(curProduct);
        await _appDbContext.SaveChangesAsync();
        if (!string.IsNullOrWhiteSpace(curProduct.ImagePath))
        {
            // delete image
            TryToDeleteFile(curProduct.ImagePath);
        }
        return NoContent();
    }

    private async Task<(string[]? Errors, string? FilePath)> TryToUploadFileAsync(IFormFile file)
    {
        // valid file that user try to uload
        var maxFileSize = Convert.ToInt32(_imageSettings["MaxFileSize"]);
        var allowedMimeTypes = _imageSettings.GetSection("AllowedMimeTypes").Get<string[]>();
        if (file.Length > maxFileSize || !allowedMimeTypes!.Contains(file.ContentType))
        {
            var errors = new[]
            {
                    $"Image size must be less than {maxFileSize/1048576} MB.",
                    $"Image file must be {string.Join(',', allowedMimeTypes!)}.",
                };
            return (errors, null);
        }
        // gerate file path
        var uploadFolder = _imageSettings["UploadFolder"];
        var fileName = Path.GetRandomFileName() + "." + file.ContentType.Split("/")[1];
        var filePath = Path.Combine(uploadFolder!, fileName);
        // try to upload file
        using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        return (null, filePath);
    }

    private void TryToDeleteFile(string ImagePath)
    {
        if (System.IO.File.Exists(ImagePath))
        {
            System.IO.File.Delete(ImagePath);
        }
    }
}

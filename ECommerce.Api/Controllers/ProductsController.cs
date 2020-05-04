using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ECommerce.DataAccess;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<IEnumerable<ProductViewModel>> GetAll()
        {
            var products = await _context.Products.ToListAsync();

            List<ProductCategory> productCategories = await _context.ProductCategories
                                                    .AsNoTracking()
                                                    .Include(x => x.Product)
                                                    .Include(x => x.Category)
                                                    .ToListAsync();

            List<ProductViewModel> productVMs = new List<ProductViewModel>();

            foreach (var product in products)
            {
                ProductViewModel productVM = new ProductViewModel
                {
                    Product = product,
                    CategoryIds = productCategories.Where(x => x.ProductId == product.Id).Select(x => x.CategoryId).ToList(),
                    Categories = new List<string>()
                };

                for (int i = 0; i < productVM.CategoryIds.Count(); ++i)
                {
                    if (i == 0)
                    {
                        productVM.CategoryId_1 = productVM.CategoryIds[0].ToString();
                    }
                    if (i == 1)
                    {
                        productVM.CategoryId_2 = productVM.CategoryIds[1].ToString();
                    }
                    if (i == 2)
                    {
                        productVM.CategoryId_3 = productVM.CategoryIds[2].ToString();
                    }
                    productVM.Categories.Add(productCategories.First(x => x.CategoryId == productVM.CategoryIds[i]).Category.Name);
                }
                productVMs.Add(productVM);
            }

            return productVMs;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductViewModel>> Get(long id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product of ID {ProductId} not found", id);
                return NotFound();
            }

            List<ProductCategory> productCategories = await _context.ProductCategories
                                                    .Where(x => x.ProductId == product.Id)
                                                    .Include(x => x.Product)
                                                    .Include(x => x.Category)
                                                    .ToListAsync();

            ProductViewModel productVM = new ProductViewModel
            {
                Product = product,
                CategoryIds = productCategories.Select(x => x.CategoryId).ToList(),
                Categories = new List<string>()
            };

            for (int i = 0; i < productVM.CategoryIds.Count(); ++i)
            {
                if (i == 0)
                {
                    productVM.CategoryId_1 = productVM.CategoryIds[0].ToString();
                }
                if (i == 1)
                {
                    productVM.CategoryId_2 = productVM.CategoryIds[1].ToString();
                }
                if (i == 2)
                {
                    productVM.CategoryId_3 = productVM.CategoryIds[2].ToString();
                }
                productVM.Categories.Add(productCategories[i].Category.Name);
            }
            return productVM;
        }

        [HttpPost]
        public async Task<ActionResult> Add(ProductViewModel productVM)
        {
            try
            {
                await _context.Products.AddAsync(productVM.Product);
                await _context.SaveChangesAsync();

                foreach (var categoryId in productVM.CategoryIds)
                {
                    await _context.ProductCategories.AddAsync(new ProductCategory
                    {
                        ProductId = productVM.Product.Id,
                        CategoryId = categoryId
                    });
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding new product {@ProductVM} to database", productVM);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ProductViewModel productVM)
        {
            if (id != productVM.Product.Id)
            {
                return BadRequest();
            }

            try
            {
                var currentProductCategoryMapping = await _context.ProductCategories.Include(x => x.Category)
                                                            .Where(x => x.ProductId == productVM.Product.Id)
                                                            .ToListAsync();
                var productAuditTrail = new ProductAuditTrail
                {
                    ProductId = productVM.Product.Id,
                    Name = productVM.Product.Name,
                    Description = productVM.Product.Description,
                    Price = productVM.Product.Price,
                    ImageUrl = productVM.Product.ImageUrl,
                    IsAvailable = productVM.Product.IsAvailable,
                    Categories = string.Join(",", currentProductCategoryMapping.Select(x => x.Category.Name).ToList()),
                    ActionTypeId = (long) SD.EntityActionType.Update,
                    PerformedBy = productVM.Product.UpdatedBy,
                    PerformedDate = productVM.Product.UpdatedAt
                };
                _context.ProductCategories.RemoveRange(currentProductCategoryMapping);
                _context.ProductAuditTrails.Add(productAuditTrail);

                foreach (var categoryId in productVM.CategoryIds)
                {
                    await _context.ProductCategories.AddAsync(new ProductCategory
                    {
                        ProductId = productVM.Product.Id,
                        CategoryId = categoryId
                    });
                }

                _context.Entry(productVM.Product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product {@ProductVM} in database", productVM);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            Product product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Attempt to delete non-existing product of ID {ProductId}", id);
                return NotFound();
            }

            try
            {
                product.IsAvailable = !product.IsAvailable; // Just mark it instead of deleting it
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing product {@Product} availability in database", product);
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }

            return NoContent();
        }
    }
}

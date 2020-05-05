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
        
        /// <summary>
        /// Get all products from the database.
        /// </summary>
        /// <returns></returns>
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
                    productVM.Categories.Add(productCategories.First(x => x.CategoryId == productVM.CategoryIds[i]).Category.Name);
                }
                productVMs.Add(productVM);
            }

            return productVMs;
        }

        /// <summary>
        /// Get a specific product from the database.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns></returns>
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
                productVM.Categories.Add(productCategories[i].Category.Name);
            }
            return productVM;
        }

        /// <summary>
        /// Add a new product to the database.
        /// </summary>
        /// <param name="productVM">ProductViewModel Object</param>
        /// <returns></returns>
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

        /// <summary>
        /// Update an existing product in the database.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="productVM">ProductViewModel Object</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ProductViewModel productVM)
        {
            if (id != productVM.Product.Id)
            {
                return BadRequest();
            }

            var productFromDb = await _context.Products.FindAsync(id);

            if (productFromDb == null)
            {
                return NotFound();
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

        /// <summary>
        /// Mark a product as not available instead of truly deleting it.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpDelete("{userId}/{id}")]
        public async Task<IActionResult> Delete(long id, string userId)
        {
            Product product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Attempt to delete non-existing product of ID {ProductId}", id);
                return NotFound();
            }

            try
            {
                var currentProductCategoryMapping = await _context.ProductCategories.Include(x => x.Category)
                                                            .Where(x => x.ProductId == product.Id)
                                                            .ToListAsync();

                var productAuditTrail = new ProductAuditTrail
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    IsAvailable = product.IsAvailable,
                    Categories = string.Join(",", currentProductCategoryMapping.Select(x => x.Category.Name).ToList()),
                    ActionTypeId = (long)SD.EntityActionType.Delete,
                    PerformedBy = userId,
                    PerformedDate = DateTime.Now
                };
                _context.ProductAuditTrails.Add(productAuditTrail);

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

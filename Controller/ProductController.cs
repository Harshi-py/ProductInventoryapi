using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductInventoryAPI.Data;
using ProductInventoryAPI.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace ProductInventoryAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductController(AppDbContext db)
        {
            _db = db;
        }
        [Authorize(Roles = "Admin,Manager,Viewer")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? category,[FromQuery] string? sortByPrice,[FromQuery] bool outOfStock = false)
        {
            IQueryable<Product> query = _db.Products;
            if(!string.IsNullOrEmpty(category)) query = query.Where(p=>p.Category == category.ToLower());

      if (outOfStock)
                query = query.Where(p => p.StockQuantity <= 0);
            if (!string.IsNullOrWhiteSpace(sortByPrice))
            {
                query = sortByPrice.ToLower() switch
                {
                    "asc" => query.OrderBy(p => p.Price),
                    "desc" => query.OrderByDescending(p => p.Price),
                    _ => query
                };
            }
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
 
            return Ok(new
            {
                calledBy = username,
                callerRole = role,
                products = await query.ToListAsync()
            });
        }
        [Authorize(Roles = "Admin,Manager,Viewer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _db.Products.FindAsync(id);
 
            if (product == null)
                return NotFound("Product not found");
 
            return Ok(product);
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid product data");
 
                product.Id = 0;
 
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
 
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            if (product == null)
                return BadRequest("Product data is invalid");
 
            if (id != product.Id)
                return BadRequest("ID in URL does not match ID in body");
 
            var existingProduct = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
 
            if (existingProduct == null)
                return NotFound("Product not found");
            existingProduct.Name = product.Name;
            existingProduct.Category = product.Category;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
 
            await _db.SaveChangesAsync();
 
            return Ok(existingProduct);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
 
            if (product == null)
                return NotFound("Product not found");
 
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
 
            return NoContent();
        }
    }
}
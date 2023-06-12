using HPLusSportAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPLusSportAPI.Controllers
{

    //[Route("api/[controller]")]  //https://localhost:7291/api/Products
    [ApiVersion("1.0")]  //https://localhost:7291/v1.0/products
    //[Route("v{v:apiVersion}/products")] For versioning on URL
    [Route("products")] // For versioning on header = https://localhost:7291/products
    [Authorize]
    [ApiController]
    public class ProductsV1Controller : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsV1Controller(ShopContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery]ProductQueryParameters queryParameters)
        {
            IQueryable<Product> products = _context.Products;

            if (queryParameters.MinPrice != null)
            {
                products = products.Where(p => p.Price >= queryParameters.MinPrice);
            }

            if (queryParameters.MaxPrice != null)
            {
                products = products.Where(p => p.Price <= queryParameters.MaxPrice);
            }

            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                products = products.Where(
                    p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                         p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
            }

            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = 
                    products
                    .Where(p => p.Name.ToLower()
                    .Contains(queryParameters.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.SortBy)) 
            {
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(
                        queryParameters.SortBy,
                        queryParameters.SortOrder);
                }
            }

            products = products.Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);

            return Ok( await products.ToArrayAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id) {
            var product = _context.Products.Find(id);
            return product != null? Ok(product) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetProduct", new {id = product.Id}, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutProduct(int id, Product product)
        {
            if(id != product.Id) { return BadRequest(); }

            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if(_context.Products.Any(p => p.Id == id)) {
                    return NotFound();
                } else { throw; }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null) { return NotFound(); }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return product;
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<ActionResult> DeleteMultiple([FromQuery]int[] ids)
        {
            var products = new List<Product>();

            foreach(var id in ids)
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null) { return NotFound(); }

                products.Add(product);
            }
            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();
            return Ok(products);

        }
    }

    //[Route("api/[controller]")]  //https://localhost:7291/api/Products
    [ApiVersion("2.0")]  //https://localhost:7291/v2.0/products
    //[Route("v{v:apiVersion}/products")] // For versioning on URL
    [Route("products")] // For versioning on header = https://localhost:7291/products
    [ApiController]
    public class ProductsV2Controller : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsV2Controller(ShopContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductQueryParameters queryParameters)
        {
            IQueryable<Product> products = 
                _context.Products.Where(p => p.IsAvailable == true);

            if (queryParameters.MinPrice != null)
            {
                products = products.Where(p => p.Price >= queryParameters.MinPrice);
            }

            if (queryParameters.MaxPrice != null)
            {
                products = products.Where(p => p.Price <= queryParameters.MaxPrice);
            }

            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                products = products.Where(
                    p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                         p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
            }

            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products =
                    products
                    .Where(p => p.Name.ToLower()
                    .Contains(queryParameters.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(
                        queryParameters.SortBy,
                        queryParameters.SortOrder);
                }
            }

            products = products.Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);

            return Ok(await products.ToArrayAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            var product = _context.Products.Find(id);
            return product != null ? Ok(product) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id) { return BadRequest(); }

            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (_context.Products.Any(p => p.Id == id))
                {
                    return NotFound();
                }
                else { throw; }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) { return NotFound(); }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return product;
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<ActionResult> DeleteMultiple([FromQuery] int[] ids)
        {
            var products = new List<Product>();

            foreach (var id in ids)
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null) { return NotFound(); }

                products.Add(product);
            }
            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();
            return Ok(products);

        }
    }
}

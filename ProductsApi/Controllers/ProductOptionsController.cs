using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsApi.Models;

namespace ProductsApi.Controllers
{
    [Route("api/products/{productId}/options")]
    [ApiController]
    public class ProductOptionsController : ControllerBase
    {
        private readonly ProductOptionContext _context;

        public ProductOptionsController(ProductOptionContext context)
        {
            _context = context;
        }

        private async Task<ProductOption> FindProductOption(Guid productId, Guid id)
        {
            return await _context.ProductOptions
                .FirstOrDefaultAsync(option => option.ProductId == productId && option.Id == id);
        }

        // GET: api/products/{productId}/options
        [HttpGet]
        public async Task<ActionResult<CollectionResult<ProductOption>>> GetProductOptions(Guid productId)
        {

            if (!ProductExists(productId))
            {
                return NotFound();
            }

            List<ProductOption> options = await _context.ProductOptions.Where(option => option.ProductId == productId).ToListAsync();

            return new CollectionResult<ProductOption>(options);
        }

        // GET: api/products/{productId}/options/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductOption>> GetProductOption(Guid productId, Guid id)
        {
            ProductOption productOption = await FindProductOption(productId, id);

            return productOption == null ? NotFound() : (ActionResult<ProductOption>)productOption;
        }

        // PUT: api/products/{productId}/options/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductOption(Guid productId, Guid id, ProductOption productOption)
        {
            if (!ProductOptionExists(id) || !ProductExists(productId))
            {
                return NotFound();
            }

            productOption.Id = id;
            productOption.ProductId = productId;
            _context.Entry(productOption).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/products/{productId}/options
        [HttpPost]
        public async Task<ActionResult<ProductOption>> PostProductOption(Guid productId, ProductOption productOption)
        {
            if (!ProductExists(productId))
            {
                return BadRequest();
            }

            productOption.ProductId = productId;
            await _context.ProductOptions.AddAsync(productOption);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductOption", new { productId, id = productOption.Id }, productOption);
        }

        // DELETE: api/products/{productId}/options/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ProductOption>> DeleteProductOption(Guid productId, Guid id)
        {
            ProductOption productOption = await FindProductOption(productId, id);
            if (productOption == null)
            {
                return NotFound();
            }

            _context.ProductOptions.Remove(productOption);
            await _context.SaveChangesAsync();

            return productOption;
        }

        private bool ProductOptionExists(Guid id)
        {
            return _context.ProductOptions.Any(e => e.Id == id);
        }
        private bool ProductExists(Guid productId)
        {
            return _context.Products.Any(e => e.Id == productId);
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ProductsApi.Models
{
    public class ProductOptionContext : DbContext
    {
        public ProductOptionContext(DbContextOptions<ProductOptionContext> options)
            : base(options)
        {
        }

        public DbSet<ProductOption> ProductOptions { get; set; }
        public DbSet<Product> Products { get; set; }
    }

    public class ProductOption
    {
        public Guid Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Description { get; set; }
        [JsonIgnore] [Required] public Guid ProductId { get; set; }
    }
}
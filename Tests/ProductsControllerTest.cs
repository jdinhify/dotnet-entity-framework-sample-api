using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ProductsApi.Controllers;
using ProductsApi.Models;
using Xunit;

namespace Tests
{
    public abstract class ProductsControllerTest
    {
        #region Seeding

        protected ProductsControllerTest(DbContextOptions<ProductContext> contextOptions)
        {
            ContextOptions = contextOptions;
            Seed();
        }

        protected DbContextOptions<ProductContext> ContextOptions { get; }

        private void Seed()
        {
            using var context = new ProductContext(ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var p1 = new Product
            {
                Name = "Product 1 Name",
                Description = "Product 1 Description",
                Price = 1.11F,
                DeliveryPrice = 1.1F
            };
            var p2 = new Product
            {
                Name = "Product 2 Name",
                Description = "Product 2 Description",
                Price = 2.22F,
                DeliveryPrice = 2.2F
            };
            var p3 = new Product
            {
                Name = "Product 3 Name",
                Description = "Product 3 Description",
                Price = 3.33F,
                DeliveryPrice = 3.3F
            };

            context.Products.AddRange(p1, p2, p3);
            context.SaveChanges();
        }
        #endregion

        #region Tests
        [Fact]
        public async Task GetsProducts()
        {
            await using var context = new ProductContext(ContextOptions);
            var controller = new ProductsController(context);

            var products = await controller.GetProducts("");

            Assert.Equal(3, products.Value.Items.Count);
        }

        [Fact]
        public async Task GetsProductsByName()
        {
            await using var context = new ProductContext(ContextOptions);
            var controller = new ProductsController(context);

            var products = await controller.GetProducts("Product 2 Name");

            Assert.Single(products.Value.Items);
        }

        [Fact]
        public async Task GetsProductDetails()
        {
            await using var context = new ProductContext(ContextOptions);
            var controller = new ProductsController(context);
            var products = await controller.GetProducts("Product 2 Name");

            var product = await controller.GetProduct(products.Value.Items[0].Id);

            Assert.Equal("Product 2 Name", product.Value.Name);
        }

        [Fact]
        public async Task CreatesNewProduct()
        {
            await using var context = new ProductContext(ContextOptions);
            var controller = new ProductsController(context);
            Product newProduct = new Product
            {
                Name = "Product 4 Name",
                Description = "Product 4 Description",
                Price = 4.44F,
                DeliveryPrice = 4.4F,
            };
            await controller.PostProduct(newProduct);

            var products = await controller.GetProducts("");
            Assert.Equal(4, products.Value.Items.Count);
        }

        [Fact]
        public async Task UpdatesProduct()
        {
            await using var context = new ProductContext(ContextOptions);
            var controller = new ProductsController(context);
            var products = await controller.GetProducts("Product 2 Name");
            var updatedProductId = products.Value.Items[0].Id;
            var updatedProduct = new Product
            {
                Name = "Product 2 Name Updated",
                Description = "Product 2 Description Updated",
                Price = 2.22F,
                DeliveryPrice = 2.2F,
            };
            updatedProduct.Id = updatedProductId;

            await controller.PutProduct(updatedProductId, updatedProduct);

            var product = await controller.GetProduct(updatedProductId);
            Assert.Equal("Product 2 Name Updated", product.Value.Name);
            Assert.Equal("Product 2 Description Updated", product.Value.Description);
        }

        [Fact]
        public async Task DeletesProduct()
        {
            await using var context = new ProductContext(ContextOptions);
            var controller = new ProductsController(context);
            var productIdToDelete = (await controller.GetProducts("Product 2 Name")).Value.Items[0].Id;

            await controller.DeleteProduct(productIdToDelete);

            var products = await controller.GetProducts("");
            Assert.Equal(2, products.Value.Items.Count);
        }
        #endregion
    }

    #region SqliteInMemory
    public class SqliteInMemoryProductsControllerTest : ProductsControllerTest, IDisposable
    {
        private readonly DbConnection _connection;

        public SqliteInMemoryProductsControllerTest()
            : base(
                new DbContextOptionsBuilder<ProductContext>()
                    .UseSqlite(CreateInMemoryDatabase())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .Options)
        {
            _connection = RelationalOptionsExtension.Extract(ContextOptions).Connection;
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");

            connection.Open();

            return connection;
        }

        public void Dispose() => _connection.Dispose();
    }
    #endregion
}
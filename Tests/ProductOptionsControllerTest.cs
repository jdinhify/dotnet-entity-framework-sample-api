using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ProductsApi.Controllers;
using ProductsApi.Models;
using Xunit;

namespace Tests
{

    public abstract class ProductOptionsControllerTest
    {
        private async Task<Guid> GetProductId(ProductOptionContext context)
        {
            return (await context.Products.FirstOrDefaultAsync()).Id;
        }

        #region Seeding

        protected ProductOptionsControllerTest(DbContextOptions<ProductOptionContext> contextOptions)
        {
            ContextOptions = contextOptions;
            SeedAsync();
        }

        protected DbContextOptions<ProductOptionContext> ContextOptions { get; }

        private async void SeedAsync()
        {
            using var context = new ProductOptionContext(ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var p1 = new Product
            {
                Name = "Product 1 Name",
                Description = "Product 1 Description",
                Price = 1.11F,
                DeliveryPrice = 1.1F
            };
            context.Products.Add(p1);
            await context.SaveChangesAsync();
            var productId = await GetProductId(context);

            var po1 = new ProductOption
            {
                ProductId = productId,
                Name = "PO 1 Name",
                Description = "PO 1 Description"
            };
            var po2 = new ProductOption
            {
                ProductId = productId,
                Name = "PO 2 Name",
                Description = "PO 2 Description"
            };
            var po3 = new ProductOption
            {
                ProductId = productId,
                Name = "PO 3 Name",
                Description = "PO 3 Description"
            };

            context.ProductOptions.AddRange(po1, po2, po3);
            context.SaveChanges();
        }
        #endregion

        #region Tests
        [Fact]
        public async Task GetsProductOptions()
        {
            await using var context = new ProductOptionContext(ContextOptions);
            var productId = await GetProductId(context);
            var controller = new ProductOptionsController(context);

            var productOptions = await controller.GetProductOptions(productId);

            Assert.Equal(3, productOptions.Value.Items.Count);
        }

        [Fact]
        public async Task GetsProductOptionDetails()
        {
            await using var context = new ProductOptionContext(ContextOptions);
            var productId = await GetProductId(context);
            var controller = new ProductOptionsController(context);
            var productOptions = await controller.GetProductOptions(productId);

            var productOption = await controller.GetProductOption(productId, productOptions.Value.Items[1].Id);

            Assert.Equal("PO 2 Name", productOption.Value.Name);
        }

        [Fact]
        public async Task CreatesNewProductOption()
        {
            await using var context = new ProductOptionContext(ContextOptions);
            var productId = await GetProductId(context);
            var controller = new ProductOptionsController(context);
            ProductOption newPO = new ProductOption
            {
                Name = "New PO Name",
                Description = "New PO Description",
            };
            await controller.PostProductOption(productId, newPO);

            var productOptions = await controller.GetProductOptions(productId);
            Assert.Equal(4, productOptions.Value.Items.Count);
        }

        [Fact]
        public async Task UpdatesProductOption()
        {
            await using var context = new ProductOptionContext(ContextOptions);
            var productId = await GetProductId(context);
            var controller = new ProductOptionsController(context);
            var productOptions = await controller.GetProductOptions(productId);

            var updatedProductOptionId = productOptions.Value.Items[1].Id;

            var updatedProductOption = new ProductOption
            {
                Name = "PO 2 Name Updated",
                Description = "PO 2 Description Updated",
            };
            updatedProductOption.Id = updatedProductOptionId;

            await controller.PutProductOption(productId, updatedProductOptionId, updatedProductOption);

            var productOption = await controller.GetProductOption(productId, updatedProductOptionId);
            Assert.Equal("PO 2 Name Updated", productOption.Value.Name);
            Assert.Equal("PO 2 Description Updated", productOption.Value.Description);
        }

        [Fact]
        public async Task DeletesProductOption()
        {
            await using var context = new ProductOptionContext(ContextOptions);
            var productId = await GetProductId(context);
            var controller = new ProductOptionsController(context);
            var productIdToDelete = (await controller.GetProductOptions(productId)).Value.Items[0].Id;

            await controller.DeleteProductOption(productId, productIdToDelete);

            var products = await controller.GetProductOptions(productId);
            Assert.Equal(2, products.Value.Items.Count);
        }
        #endregion
    }

    #region SqliteInMemory
    public class SqliteInMemoryProductOptionsControllerTest : ProductOptionsControllerTest, IDisposable
    {
        private readonly DbConnection _connection;

        public SqliteInMemoryProductOptionsControllerTest()
            : base(
                new DbContextOptionsBuilder<ProductOptionContext>()
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
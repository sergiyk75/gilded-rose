using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GildedRoseWebApplication.Controllers;
using GildedRoseWebApplication.Models;
using GildedRoseWebApplication.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace GildedRoseWebApplication.Tests.Controllers
{
    public class InventoryControllerIntegrationTests
    {
        class TestStartup : Startup
        {
            public TestStartup(IHostingEnvironment env)
                : base(env)
            {
            }


            public override void ConfigureCustomServices(IServiceCollection services)
            {
                // do not register inventory service as we override it below
            }
        }

        static class ControllerPath
        {
            public static readonly string ListInventory = "/api/inventory";
            public static readonly string GetByID = "/api/inventory/{0}";
            public static readonly string BuyProductsPath = "/api/inventory/{0}/{1}";
            public static readonly string BuyProductPath = "/api/inventory/{0}";
            public static readonly string GetAuthorizationTokenPath = "/api/token";
        }

        private static readonly string AuthenticationScheme = "Bearer";

        private readonly TestServer server;
        private readonly HttpClient client;

        private static readonly InventoryItem expectedItem1 = new InventoryItem { Product = new Product { ID = "product_1" }, StockCount = 10 };
        private readonly InventoryItem expectedItem2 = new InventoryItem { Product = new Product { ID = "product_2" }, StockCount = 20 };

        public InventoryControllerIntegrationTests()
        {
            // Arrange
            // Normally, integration tests should run with unmodified stratup stack
            // However, here for demonstration purposes I will use in-memory inventory service with predefined items 
            server = new TestServer(new WebHostBuilder()
                .UseStartup<TestStartup>()
                .ConfigureServices(InitializeServices));

            client = server.CreateClient();
        }

        private void InitializeServices(IServiceCollection services)
        {
            var inventoryService = new InMemoryInventoryService()
                .Add(expectedItem1)
                .Add(expectedItem2);

            services.AddSingleton<IInventoryService>(inventoryService);
        }
        
        private static bool InventoryItemsEqual(InventoryItem first, InventoryItem second)
        {
            return first.Product.ID == second.Product.ID;
        }

        [Fact]
        public async Task GetAllTest()
        {
            // Act
            var response = await client.GetAsync(ControllerPath.ListInventory);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var inventory = JsonConvert.DeserializeObject<InventoryItem[]>(json);

            Assert.Equal(2, inventory.Length);
            Assert.True(inventory.Any(item => InventoryItemsEqual(item, expectedItem1)));
            Assert.True(inventory.Any(item => InventoryItemsEqual(item, expectedItem2)));
        }

        [Theory]
        [InlineData("product_1")]
        [InlineData("product_2")]
        public async Task GetByIDTestOK(string productID)
        {
            // Act
            var response = await client.GetAsync(string.Format(ControllerPath.GetByID, productID));

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<InventoryItem>(json);

            Assert.Equal(productID, item.Product.ID);
        }

        [Fact]
        public async Task GetByIDTestFailed()
        {
            // Act
            var response = await client.GetAsync(string.Format(ControllerPath.GetByID, "non-existing-product-ID"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("product_1")]
        [InlineData("product_2")]
        public async Task BuyTestUnauthorized(string productID)
        {
            // Arrange

            var httpContent = new StringContent("");

            // Act
            var response = await client.PutAsync(string.Format(ControllerPath.BuyProductPath, productID), httpContent);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("product_1", null)]
        [InlineData("product_1", 4)]
        [InlineData("product_2", null)]
        [InlineData("product_2", 2)]
        public async Task BuyTestOK(string productID, int? count)
        {
            // Arrange
            await ObtainAutharizationToken();

            var httpContent = new StringContent("");

            var requestUri = count.HasValue ?
                string.Format(ControllerPath.BuyProductsPath, productID, count.Value) :
                string.Format(ControllerPath.BuyProductPath, productID);
            // Act
            var response = await client.PutAsync(requestUri, httpContent);

            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData("product_1", 20)]
        [InlineData("product_2", 30)]
        public async Task BuyTestFailedInsufficientStockLevel(string productID, int count)
        {
            // Arrange
            await ObtainAutharizationToken();

            var httpContent = new StringContent("");
            // Act
            // try to buy more products than stock count
            var response = await client.PutAsync(string.Format(ControllerPath.BuyProductsPath, productID, count), httpContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var itemErrorResponse = JsonConvert.DeserializeObject<InventoryController.ItemErrorResponse>(json);

            Assert.Equal("Insufficient stock level", itemErrorResponse.Error);

            Assert.Equal(productID, itemErrorResponse.Item.Product.ID);
        }
        
        private async Task ObtainAutharizationToken()
        {
            var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", "user" },
                { "password", "123" }
            });

            var response = await client.PostAsync(ControllerPath.GetAuthorizationTokenPath, httpContent);
            var json = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, values["access_token"]);
        }
    }
}

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

        private static HttpClient StartServerAndReturnClient()
        {
            // Arrange
            // Normally, integration tests should run with unmodified stratup stack
            // However, here for demonstration purposes I will use in-memory inventory service with predefined items 
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<TestStartup>()
                .ConfigureServices(InitializeServices));

            return server.CreateClient();
        }

        private static void InitializeServices(IServiceCollection services)
        {
            var inventoryService = new InMemoryInventoryService()
                .Add(new InventoryItem { Product = new Product { ID = "product_1" }, StockCount = 10 })
                .Add(new InventoryItem { Product = new Product { ID = "product_2" }, StockCount = 20 });

            services.AddSingleton<IInventoryService>(inventoryService);
        }
        
        [Fact]
        public async Task GetAllItems()
        {
            using (var client = StartServerAndReturnClient())
            {
                // Act
                var response = await client.GetAsync(ControllerPath.ListInventory);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // Assert
                var inventoryItems = JsonConvert.DeserializeObject<InventoryItem[]>(json);

                Assert.Equal(2, inventoryItems.Length);
                Assert.True(inventoryItems.Any(item => item.Product.ID == "product_1"));
                Assert.True(inventoryItems.Any(item => item.Product.ID == "product_2"));
            }
        }

        [Theory]
        [InlineData("product_1")]
        [InlineData("product_2")]
        public async Task GetItem_OK(string productID)
        {
            using (var client = StartServerAndReturnClient())
            {

                // Act
                var response = await client.GetAsync(string.Format(ControllerPath.GetByID, productID));

                response.EnsureSuccessStatusCode();

                // Assert
                var json = await response.Content.ReadAsStringAsync();
                var item = JsonConvert.DeserializeObject<InventoryItem>(json);

                Assert.Equal(productID, item.Product.ID);
            }
        }

        [Fact]
        public async Task GetItem_Failed()
        {
            using (var client = StartServerAndReturnClient())
            {
                // Act
                var response = await client.GetAsync(string.Format(ControllerPath.GetByID, "non-existing-product-ID"));

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Theory]
        [InlineData("product_1")]
        [InlineData("product_2")]
        public async Task Buy_Unauthorized(string productID)
        {
            using (var client = StartServerAndReturnClient())
            {
                // Arrange

                var httpContent = new StringContent("");

                // Act
                var response = await client.PutAsync(string.Format(ControllerPath.BuyProductPath, productID), httpContent);

                // Assert
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Theory]
        [InlineData("product_1", null)]
        [InlineData("product_1", 10)]
        [InlineData("product_2", null)]
        [InlineData("product_2", 20)]
        public async Task BuyItem_OK(string productID, int? count)
        {
            using (var client = StartServerAndReturnClient())
            {
                // Arrange
                await ObtainAutharizationToken(client);

                var httpContent = new StringContent("");

                var requestUri = count.HasValue ?
                    string.Format(ControllerPath.BuyProductsPath, productID, count.Value) :
                    string.Format(ControllerPath.BuyProductPath, productID);
                // Act
                var response = await client.PutAsync(requestUri, httpContent);

                // Assert
                response.EnsureSuccessStatusCode();
            }
        }

        [Theory]
        [InlineData("product_1", 20)]
        [InlineData("product_2", 30)]
        public async Task BuyItem_FailedInsufficientStockLevel(string productID, int count)
        {
            using (var client = StartServerAndReturnClient())
            {
                // Arrange
                await ObtainAutharizationToken(client);

                var httpContent = new StringContent("");
                // Act
                // try to buy more products than stock count
                var response = await client.PutAsync(string.Format(ControllerPath.BuyProductsPath, productID, count), httpContent);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                var json = await response.Content.ReadAsStringAsync();
                var responseValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                Assert.Equal("Insufficient stock level", responseValues["error"]);

                Assert.Equal(productID, responseValues["productID"]);
            }
        }
        
        private async Task ObtainAutharizationToken(HttpClient client)
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

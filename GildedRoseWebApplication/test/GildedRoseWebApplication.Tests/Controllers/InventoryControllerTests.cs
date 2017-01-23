using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GildedRoseWebApplication.Services;
using Xunit;
using NSubstitute;

using System.Threading;
using GildedRoseWebApplication.Models;
using GildedRoseWebApplication.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GildedRoseWebApplication.Tests.Controllers
{
    public class InventoryControllerTests
    {
        [Fact]
        public async Task GetAllItems()
        {
            // Arrange
            var expectedCancellationToken = CancellationToken.None;
            var expectedItem = new InventoryItem();
            IEnumerable<InventoryItem> inventoryItems = new List<InventoryItem> { expectedItem };
            var mockInventoryService = Substitute.For<IInventoryService>();
            mockInventoryService
                .GetAllAsync(expectedCancellationToken)
                .Returns(Task.FromResult(inventoryItems));

            var subject = new InventoryController(mockInventoryService);

            // Act
            var result = await subject.GetAllItems(expectedCancellationToken);

            Assert.Equal(1, result.Count());
            Assert.Same(expectedItem, result.First());

        }

        [Fact]
        public async Task GetItem()
        {
            // Arrange
            var expectedCancellationToken = CancellationToken.None;
            var expectedItem = new InventoryItem { Product = new Product { ID = "whatever" } };
            var mockInventoryService = Substitute.For<IInventoryService>();
            mockInventoryService
                .FindAsync(expectedItem.Product.ID, expectedCancellationToken)
                .Returns(Task.FromResult(expectedItem));

            var subject = new InventoryController(mockInventoryService);

            // Act
            var result = await subject.GetItem(expectedItem.Product.ID, expectedCancellationToken);

            //Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expectedItem, okObjectResult.Value);
        }

        [Fact]
        public async Task GetItems_NotFound()
        {
            // Arrange
            var expectredProductID = "whatever";
            var expectedCancellationToken = CancellationToken.None;
            var mockInventoryService = Substitute.For<IInventoryService>();
            mockInventoryService
                .FindAsync(expectredProductID, expectedCancellationToken)
                .Returns(Task.FromResult((InventoryItem)null));

            var subject = new InventoryController(mockInventoryService);

            // Act
            var result = await subject.GetItem(expectredProductID, expectedCancellationToken);

            //Assert
            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(expectredProductID, notFoundObjectResult.Value);
        }

        [Fact]
        public async Task BuyItem()
        {
            // Arrange
            var expectedProductID = "whatever";
            var expectedProductCount = 10;
            var expectedCancellationToken = CancellationToken.None;
            var mockInventoryService = Substitute.For<IInventoryService>();
            mockInventoryService
                .FindAsync(expectedProductID, expectedCancellationToken)
                .Returns(Task.FromResult(new InventoryItem { Product = new Product { ID = expectedProductID }, StockCount = 20 }));
            mockInventoryService
                .BuyAsync(expectedProductID, expectedProductCount, expectedCancellationToken)
                .Returns(Task.FromResult(true));

            var subject = new InventoryController(mockInventoryService);

            // Act
            var result = await subject.BuyItem(expectedProductID, expectedProductCount, expectedCancellationToken);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task BuyItem_NotFound()
        {
            // Arrange
            var expectedProductID = "whatever";
            var expectedProductCount = 10;
            var expectedCancellationToken = CancellationToken.None;
            var mockInventoryService = Substitute.For<IInventoryService>();
            mockInventoryService
                .FindAsync(expectedProductID, expectedCancellationToken)
                .Returns(Task.FromResult((InventoryItem)null));

            var subject = new InventoryController(mockInventoryService);

            // Act
            var result = await subject.BuyItem(expectedProductID, expectedProductCount, expectedCancellationToken);

            //Assert
            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(expectedProductID, notFoundObjectResult.Value);
        }

        [Fact]
        public async Task BuyItem_InsuficientStockCount()
        {
            // Arrange
            var expectedProductID = "whatever";
            var expectedProductCount = 10;
            var expectedCancellationToken = CancellationToken.None;
            var mockInventoryService = Substitute.For<IInventoryService>();
            mockInventoryService
                .FindAsync(expectedProductID, expectedCancellationToken)
                .Returns(Task.FromResult(new InventoryItem { Product = new Product { ID = expectedProductID }, StockCount = 1 }));
            mockInventoryService
                .BuyAsync(expectedProductID, expectedProductCount, expectedCancellationToken)
                .Returns(Task.FromResult(true));

            var subject = new InventoryController(mockInventoryService);

            // Act
            var result = await subject.BuyItem(expectedProductID, expectedProductCount, expectedCancellationToken);

            //Assert
            var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result);
            var responsValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(badRequestObjectResult.Value));

            Assert.Equal("Insufficient stock level", responsValues["error"]);
            Assert.Equal(expectedProductID, responsValues["productID"]);
        }
    }
}

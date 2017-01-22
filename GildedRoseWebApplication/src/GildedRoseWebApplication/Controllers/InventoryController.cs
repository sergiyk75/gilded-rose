using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GildedRoseWebApplication.Models;
using GildedRoseWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GildedRoseWebApplication.Controllers
{
    /// <summary>
    /// Inventory controller
    /// </summary>
    [Route("api/[controller]")]
    public class InventoryController : Controller
    {
        private IInventoryService itemsSevice;

        public InventoryController(IInventoryService inventoryService)
        {
            this.itemsSevice = inventoryService;
        }

        /// <summary>
        /// Lists all inventory items
        /// </summary>
        /// <returns>List of all inventory items</returns>
        /// <example> 
        /// GET api/inventory
        /// </example> 
        [HttpGet]
        public IEnumerable<InventoryItem> GetAll()
        {
            return itemsSevice.GetAll();
        }

        /// <summary>
        /// Returns inventory item information if product exists
        /// otherwise 404 status
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Inventory item information</returns>
        /// <exmaple>
        /// GET api/inventory/5
        /// </exmaple>
        [HttpGet("{id}")]
        public IActionResult GetByID(string id)
        {
            var item = itemsSevice.Find(id);
            if (item == null)
            {
                return NotFound(id);
            }

            return Ok(item);
        }

        /// <summary>
        /// Buys requested count of specified product
        /// Returns 404 if specified product does not exist
        /// Returns 400 if insufficient stock level 
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="count">Requested buy count</param>
        /// <returns>Action result</returns>
        /// <remarks>User must be authenticated, i.e. have authentication token</remarks>
        /// <example>
        /// PUT api/inventory/5
        /// PUT api/inventory
        /// </example> 
        [Authorize]
        [HttpPut("{id}/{count}")]
        [HttpPut("{id}")]
        public IActionResult Buy(string id, int? count)
        {
            var item = itemsSevice.Find(id);
            if (item == null)
            {
                return NotFound(id);
            }

            var buyCount = count.GetValueOrDefault(1);

            if (item.StockCount < buyCount)
            {
                return BadRequest(new { Item = item, Error = "Insufficient stock level" });
            }

            itemsSevice.Buy(id, buyCount);

            return NoContent();
        }
    }
}

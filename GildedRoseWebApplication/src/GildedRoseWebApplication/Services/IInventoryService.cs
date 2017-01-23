using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GildedRoseWebApplication.Models;

namespace GildedRoseWebApplication.Services
{
    public interface IInventoryService
    {
        /// <summary>
        /// Lists all inventory items
        /// </summary>
        /// <returns>List of inventory items</returns>
        Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Finds an inventory item by product ID
        /// </summary>
        /// <param name="productID">Product ID</param>
        /// <returns>Inventory item</returns>
        Task<InventoryItem> FindAsync(string productID, CancellationToken cancellationToken);
        /// <summary>
        /// Finds requested inventory item by product ID and decreases its stock count by the requested buy count
        /// </summary>
        /// <param name="productID">Product ID</param>
        /// <param name="count">Requested buy count</param>
        /// <returns>True if operation is successful. False otherwise</returns>
        Task<bool> BuyAsync(string productID, int count, CancellationToken cancellationToken);
    }
}

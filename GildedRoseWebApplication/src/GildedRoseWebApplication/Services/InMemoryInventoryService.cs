using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GildedRoseWebApplication.Models;

namespace GildedRoseWebApplication.Services
{
    /// <summary>
    /// In memory implementation of inventory service for demonstration purposes
    /// Unit testing is uneccessary 
    /// Things to consider: Gloabilization and localization: strings translation, price units
    /// </summary>
    public class InMemoryInventoryService : IInventoryService
    {
        private ConcurrentDictionary<string, InventoryItem> inventory = new ConcurrentDictionary<string, InventoryItem>();

         public static IInventoryService Create()
        {
            return new InMemoryInventoryService()
                .Add(new Product { ID = "@rock", Name = "Rock", Description = "Rock beats Paper", Price = 5 }, 2)
                .Add(new Product { ID = "@paper", Name = "Paper", Description = "Paper beats Rock", Price = 10 }, 4)
                .Add(new Product { ID = "@scissors", Name = "Scissors", Description = "Scissors beats Paper", Price = 14 }, 1);
        }

        public InMemoryInventoryService Add(Product product, int stockCount)
        {
            return Add(new InventoryItem { Product = product, StockCount = stockCount });
        }

        public InMemoryInventoryService Add(InventoryItem item)
        {
            inventory[item.Product.ID] = item;
            return this;
        }

        public Task<InventoryItem> FindAsync(string productID, CancellationToken cancellationToken)
        {
            if (productID == null)
            {
                throw new ArgumentNullException("productID");
            }

            return Task.Run(() =>
            {
                InventoryItem item;
                if (inventory.TryGetValue(productID, out item))
                {
                    return item;
                }
                return null;
            });
        }

        public Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => (IEnumerable<InventoryItem>)inventory.Values);
        }

        public Task<bool> BuyAsync(string productID, int count, CancellationToken cancellationToken)
        {
            if (productID == null)
            {
                throw new ArgumentNullException("productID");
            }

            return Task.Run(() =>
            {
                if (count <= 0)
                {
                    throw new ArgumentOutOfRangeException("count");
                }

                var item = inventory[productID];
                if (item.StockCount < count)
                    return false;

                // let's make sure that buy transaction is thread safe
                // for demonstration purposes I am using lock on the item instance 
                // which technically is not very reliable as item instance is publicly accessable
                // in production a private lock object instance associated with an item must be used instead
                lock (item)
                {
                    if (item.StockCount < count)
                        return false;
                    item.StockCount -= count;
                }

                return true;
            });
        }
    }
}

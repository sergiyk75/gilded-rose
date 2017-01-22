﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GildedRoseWebApplication.Models;

namespace GildedRoseWebApplication.Services
{
    /// <summary>
    /// In memory implementation of inventory service for demonstration purposes
    /// Things to consider: Gloabilization and localization: strings translation, price units
    /// </summary>
    public class InMemoryInventoryService : IInventoryService
    {
        private static Dictionary<string, InventoryItem> inventory = new Dictionary<string, InventoryItem>();

        public InMemoryInventoryService()
        {
            Add(new Product { ID = "@rock", Name = "Rock", Description = "Rock beats Paper", Price = 5 }, 2);
            Add(new Product { ID = "@paper", Name = "Paper", Description = "Paper beats Rock", Price = 10 }, 4);
            Add(new Product { ID = "@scissors", Name = "Scissors", Description = "Scissors beats Paper", Price = 14 }, 1);
        }

        private void Add(Product product, int stockCount)
        {
            inventory[product.ID] = new InventoryItem { Product = product, StockCount = stockCount };
        }

        public InventoryItem Find(string productID)
        {
            if (productID == null)
            {
                throw new ArgumentNullException("productID");
            }

            InventoryItem item;
            if (inventory.TryGetValue(productID, out item))
            {
                return item;
            }
            return null;
        }

        public IEnumerable<InventoryItem> GetAll()
        {
            return inventory.Values;
        }

        public bool Buy(string productID, int count)
        {
            if (productID == null)
            {
                throw new ArgumentNullException("productID");
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var item = inventory[productID];
            if (item.StockCount < count)
                return false;
            item.StockCount -= count;
            return true; 
        }
    }
}

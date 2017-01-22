using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GildedRoseWebApplication.Models
{
    public class InMemoryItemsRepository : IItemsRepository
    {
        private static ConcurrentDictionary<string, Item> items = new ConcurrentDictionary<string, Item>();

        public InMemoryItemsRepository()
        {
            Add(new Item { Name = "Rock", Description = "Rock beats Paper", Price = 1 });
            Add(new Item { Name = "Paper", Description = "Paper beats Rock", Price = 2 });
            Add(new Item { Name = "Scissors", Description = "Scissors beats Paper", Price = 3 });
        }

        public void Add(Item item)
        {
            item.ID = "@" + item.Name.ToLower();
            items[item.ID] = item;
        }

        public Item Find(string itemID)
        {
            Item item;
            if (items.TryGetValue(itemID, out item))
                return item;
            return null;
        }

        public IEnumerable<Item> GetAll()
        {
            return items.Values;
        }

        public Item Remove(string itemID)
        {
            throw new NotImplementedException();
        }

        public void Update(Item item)
        {
            throw new NotImplementedException();
        }
    }
}

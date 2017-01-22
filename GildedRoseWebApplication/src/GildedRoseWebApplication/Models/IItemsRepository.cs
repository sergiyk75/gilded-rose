using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GildedRoseWebApplication.Models
{
    public interface IItemsRepository
    {
        void Add(Item item);
        IEnumerable<Item> GetAll();
        Item Find(string itemID);
        Item Remove(string itemID);
        void Update(Item item);
    }
}
